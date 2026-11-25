using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Tests.Models.Workout;

public class UnfinishedAutosaverTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private FakeTime time = null!;
    private RecordingService service = null!;
    private UnfinishedAutosaver autosaver = null!;

    [SetUp]
    public void SetUp()
    {
        time = new FakeTime();
        service = new RecordingService();
        autosaver = new UnfinishedAutosaver(service, time);
    }

    [TearDown]
    public void TearDown() => autosaver.Dispose();

    [Test]
    public async Task Queue_WritesTheWorkoutOnceTheWindowHasPassed()
    {
        autosaver.Begin();

        autosaver.Queue(Workout("a"));
        await WaitForDelayAsync();
        time.Advance(UnfinishedAutosaver.Window - TimeSpan.FromMilliseconds(1));
        Assert.That(service.Written, Is.Empty);
        time.Advance(TimeSpan.FromMilliseconds(1));
        await service.WaitForWritesAsync(1);

        Assert.That(service.Written.Select(w => w.Note), Is.EqualTo(new[] { "a" }));
    }

    [Test]
    public async Task Queue_ManyChangesInTheWindow_WritesOnlyTheNewestOnce()
    {
        autosaver.Begin();

        autosaver.Queue(Workout("a"));
        await WaitForDelayAsync();
        autosaver.Queue(Workout("b"));
        autosaver.Queue(Workout("c"));
        Assert.That(time.Created, Is.EqualTo(1), "one window serves all three changes");
        time.Advance(UnfinishedAutosaver.Window);
        await service.WaitForWritesAsync(1);
        await autosaver.FlushAsync();

        Assert.That(service.Written.Select(w => w.Note), Is.EqualTo(new[] { "c" }));
    }

    [Test]
    public async Task Queue_AfterAWrite_StartsANewWindow()
    {
        autosaver.Begin();
        autosaver.Queue(Workout("a"));
        await WaitForDelayAsync();
        time.Advance(UnfinishedAutosaver.Window);
        await service.WaitForWritesAsync(1);

        autosaver.Queue(Workout("b"));
        await WaitForDelayAsync(2);
        time.Advance(UnfinishedAutosaver.Window);
        await service.WaitForWritesAsync(2);

        Assert.That(service.Written.Select(w => w.Note), Is.EqualTo(new[] { "a", "b" }));
    }

    [Test]
    public async Task Queue_BeforeBegin_IsIgnored()
    {
        autosaver.Queue(Workout("a"));

        await autosaver.FlushAsync();

        Assert.That(service.Written, Is.Empty);
    }

    [Test]
    public async Task Begin_DropsAChangeLeftFromAnEarlierWorkout()
    {
        autosaver.Begin();
        autosaver.Queue(Workout("old"));

        autosaver.Begin();
        await autosaver.FlushAsync();

        Assert.That(service.Written, Is.Empty);
    }

    [Test]
    public async Task FlushAsync_WritesTheQueuedWorkoutAtOnce()
    {
        autosaver.Begin();
        autosaver.Queue(Workout("a"));
        autosaver.Queue(Workout("b"));

        await autosaver.FlushAsync();

        Assert.That(service.Written.Select(w => w.Note), Is.EqualTo(new[] { "b" }));
    }

    [Test]
    public async Task FlushAsync_NothingQueued_WritesNothing()
    {
        autosaver.Begin();

        await autosaver.FlushAsync();

        Assert.That(service.Written, Is.Empty);
    }

    [Test]
    public async Task FlushAsync_ThenTheWindowPasses_DoesNotWriteAgain()
    {
        autosaver.Begin();
        autosaver.Queue(Workout("a"));
        await WaitForDelayAsync();
        await autosaver.FlushAsync();

        time.Advance(UnfinishedAutosaver.Window);
        await autosaver.FlushAsync();

        Assert.That(service.Written, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task StopAsync_DropsTheQueuedWorkout()
    {
        autosaver.Begin();
        autosaver.Queue(Workout("a"));

        await autosaver.StopAsync();
        await autosaver.FlushAsync();

        Assert.That(service.Written, Is.Empty);
    }

    [Test]
    public async Task StopAsync_WaitsForAWriteInProgress()
    {
        service.Hold = new TaskCompletionSource();
        autosaver.Begin();
        autosaver.Queue(Workout("a"));
        var flush = autosaver.FlushAsync();

        var stop = autosaver.StopAsync();

        Assert.That(stop.IsCompleted, Is.False);
        service.Hold.SetResult();
        await Task.WhenAll(flush, stop);
    }

    [Test]
    public async Task Queue_AfterStopAsync_IsIgnoredUntilBegin()
    {
        autosaver.Begin();
        await autosaver.StopAsync();

        autosaver.Queue(Workout("ignored"));
        await autosaver.FlushAsync();
        autosaver.Begin();
        autosaver.Queue(Workout("kept"));
        await autosaver.FlushAsync();

        Assert.That(service.Written.Select(w => w.Note), Is.EqualTo(new[] { "kept" }));
    }

    [Test]
    public async Task AFailedWrite_ThrowsNothingAndTheNextChangeIsWrittenAgain()
    {
        service.Result = UnfinishedWrite.Failed;
        autosaver.Begin();
        autosaver.Queue(Workout("a"));
        await autosaver.FlushAsync();

        service.Result = UnfinishedWrite.Done;
        autosaver.Queue(Workout("b"));
        await autosaver.FlushAsync();

        Assert.That(service.Written.Select(w => w.Note), Is.EqualTo(new[] { "a", "b" }));
    }

    /// <summary>Waits until <paramref name="timers"/> timers are waiting on the clock, so advancing it fires them.</summary>
    private async Task WaitForDelayAsync(int timers = 1)
    {
        var deadline = Environment.TickCount64 + SpinLimitMilliseconds;
        while (time.Created < timers)
        {
            if (Environment.TickCount64 > deadline)
            {
                Assert.Fail($"{timers} timer(s) were not created: {time.Created}");
            }

            await Task.Yield();
        }
    }

    private const int SpinLimitMilliseconds = 10_000;

    private static UnfinishedWorkout Workout(string note) => new(Start, note, [new UnfinishedExercise(6, null, false, [])]);

    private sealed class FakeTime : FakeTimeProvider
    {
        private int created;

        public int Created => Volatile.Read(ref created);

        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            Interlocked.Increment(ref created);
            return base.CreateTimer(callback, state, dueTime, period);
        }
    }

    private sealed class RecordingService : IUnfinishedWorkoutService
    {
        private readonly List<UnfinishedWorkout> written = [];
        private readonly Lock gate = new();

        public IReadOnlyList<UnfinishedWorkout> Written
        {
            get
            {
                lock (gate)
                {
                    return [.. written];
                }
            }
        }

        public UnfinishedWrite Result { get; set; } = UnfinishedWrite.Done;

        /// <summary>When set, each write waits for it.</summary>
        public TaskCompletionSource? Hold { get; set; }

        public async Task WaitForWritesAsync(int count)
        {
            var deadline = Environment.TickCount64 + SpinLimitMilliseconds;
            while (Written.Count < count)
            {
                if (Environment.TickCount64 > deadline)
                {
                    Assert.Fail($"{count} write(s) were expected: {Written.Count}");
                }

                await Task.Yield();
            }
        }

        public async Task<UnfinishedWrite> SaveAsync(UnfinishedWorkout workout)
        {
            lock (gate)
            {
                written.Add(workout);
            }

            if (Hold is { } hold)
            {
                await hold.Task;
            }

            return Result;
        }

        public Task<UnfinishedSummaryResult> GetSummaryAsync() => throw new NotSupportedException();

        public Task<UnfinishedWorkoutResult> GetAsync() => throw new NotSupportedException();

        public Task<UnfinishedWrite> DiscardAsync() => throw new NotSupportedException();
    }
}
