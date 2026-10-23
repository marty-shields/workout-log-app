using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Components.Layout;
using WorkoutLog.UI.Screens;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Screens;

public class NewWorkoutPageTests : ComponentTestBase
{
    /// <summary>Tue 20 Oct 2026 23:30 UTC; Wed 21 Oct 00:30 in the test's UTC+1 zone.</summary>
    private static readonly DateTimeOffset Start = new(2026, 10, 20, 23, 30, 0, TimeSpan.Zero);

    private RecordingClock clock = null!;

    [SetUp]
    public void SetUp()
    {
        clock = new RecordingClock(Start);
        clock.SetLocalTimeZone(TimeZoneInfo.CreateCustomTimeZone("UTC+1", TimeSpan.FromHours(1), "UTC+1", "UTC+1"));
        Context.Services.AddSingleton<TimeProvider>(clock);
    }

    [Test]
    public void Open_StartsOneWorkout()
    {
        var workouts = UseWorkouts(clock);
        var cut = Context.Render<NewWorkoutPage>();

        clock.Advance(TimeSpan.FromSeconds(1));

        cut.WaitForAssertion(() => Assert.That(cut.RenderCount, Is.GreaterThan(1)));
        Assert.That(workouts.Started, Is.EqualTo(1));
    }

    [Test]
    public void Open_ShowsTheStartDateInThePhonesTimeZone()
    {
        UseWorkouts(clock);

        var cut = Context.Render<NewWorkoutPage>();

        Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Wed 21 Oct 2026"));
    }

    [Test]
    public void Timer_CountsFromTheStart()
    {
        UseWorkouts(clock);
        var cut = Context.Render<NewWorkoutPage>();
        Assert.That(Subtitle(cut), Is.EqualTo("Elapsed 0:00:00"));

        clock.Advance(TimeSpan.FromSeconds(1));
        cut.WaitForAssertion(() => Assert.That(Subtitle(cut), Is.EqualTo("Elapsed 0:00:01")));

        clock.Advance(new TimeSpan(1, 2, 3));
        cut.WaitForAssertion(() => Assert.That(Subtitle(cut), Is.EqualTo("Elapsed 1:02:04")));
    }

    [Test]
    public void ClockBeforeTheStart_ShowsZero()
    {
        UseWorkouts(new FakeTimeProvider(Start.AddMinutes(5)));

        var cut = Context.Render<NewWorkoutPage>();
        clock.Advance(TimeSpan.FromSeconds(1));

        cut.WaitForAssertion(() => Assert.That(cut.RenderCount, Is.GreaterThan(1)));
        Assert.That(Subtitle(cut), Is.EqualTo("Elapsed 0:00:00"));
    }

    [Test]
    public async Task Back_RaisesOnBack()
    {
        UseWorkouts(clock);
        var backs = 0;
        var cut = Context.Render<NewWorkoutPage>(p => p.Add(x => x.OnBack, () => backs++));

        cut.Find("[data-testid=back]").Click();
        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.That(backs, Is.EqualTo(2));
    }

    [Test]
    public async Task Disposed_DisposesTheTimer()
    {
        UseWorkouts(clock);
        Context.Render<NewWorkoutPage>();
        Assert.That(clock.Timers, Has.Count.EqualTo(1));
        Assert.That(clock.Timers[0].Disposed, Is.False);

        await Context.DisposeComponentsAsync();

        Assert.That(clock.Timers[0].Disposed, Is.True);
    }

    [Test]
    public void TickFails_ShowsTheErrorScreen()
    {
        UseWorkouts(clock);
        var cut = Context.Render<AppErrorBoundary>(p => p
            .Add(x => x.OnRestart, () => { })
            .AddChildContent<NewWorkoutPage>());

        clock.FailReads = true;
        clock.Advance(TimeSpan.FromSeconds(1));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=error-message]"), Has.Count.EqualTo(1)));
    }

    private static string Subtitle(IRenderedComponent<NewWorkoutPage> cut) => cut.Find("[data-testid=screen-subtitle]").TextContent;

    /// <summary>Registers a workout service whose drafts start at <paramref name="draftClock"/>'s current time.</summary>
    private CountingWorkoutService UseWorkouts(TimeProvider draftClock)
    {
        var workouts = new CountingWorkoutService(new SampleWorkoutService(draftClock));
        Context.Services.AddSingleton<IWorkoutService>(workouts);
        return workouts;
    }

    /// <summary>A <see cref="FakeTimeProvider"/> that keeps the timers it creates and can fail reads of the time.</summary>
    private sealed class RecordingClock(DateTimeOffset start) : FakeTimeProvider(start)
    {
        public List<RecordingTimer> Timers { get; } = [];

        /// <summary>When set, <see cref="GetUtcNow"/> throws.</summary>
        public bool FailReads { get; set; }

        public override DateTimeOffset GetUtcNow() =>
            FailReads ? throw new InvalidOperationException("Clock read failed") : base.GetUtcNow();

        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            var timer = new RecordingTimer(base.CreateTimer(callback, state, dueTime, period));
            Timers.Add(timer);
            return timer;
        }
    }

    /// <summary>Records whether it was disposed; the rest is passed to <paramref name="inner"/>.</summary>
    private sealed class RecordingTimer(ITimer inner) : ITimer
    {
        public bool Disposed { get; private set; }

        public bool Change(TimeSpan dueTime, TimeSpan period) => inner.Change(dueTime, period);

        public void Dispose()
        {
            Disposed = true;
            inner.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            return inner.DisposeAsync();
        }
    }

    /// <summary>Counts <see cref="StartNew"/> calls; the rest is passed to <paramref name="inner"/>.</summary>
    private sealed class CountingWorkoutService(IWorkoutService inner) : IWorkoutService
    {
        public int Started { get; private set; }

        public WorkoutDraft StartNew()
        {
            Started++;
            return inner.StartNew();
        }

        public WorkoutErrors Validate(IReadOnlyList<WorkoutExercise> exercises) => inner.Validate(exercises);

        public ExerciseErrors ValidateSet(int exerciseId, StrengthSet set) => inner.ValidateSet(exerciseId, set);

        public ExerciseErrors ValidateEntry(int exerciseId, CardioEntry entry) => inner.ValidateEntry(exerciseId, entry);

        public Task<SaveResult> SaveNewAsync(WorkoutDraft draft, IReadOnlyList<WorkoutExercise> exercises) => inner.SaveNewAsync(draft, exercises);

        public Task<SaveResult> SaveAsync(int workoutId, IReadOnlyList<WorkoutExercise> exercises) => inner.SaveAsync(workoutId, exercises);
    }
}
