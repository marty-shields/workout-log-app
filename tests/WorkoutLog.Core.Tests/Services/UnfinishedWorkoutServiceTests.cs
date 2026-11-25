using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;

namespace WorkoutLog.Core.Tests.Services;

public class UnfinishedWorkoutServiceTests
{
    private const int Press = 1;
    private const int Treadmill = 300;
    private const string Sentinel = "sentinel-text-4c1d";
    private static readonly DateTimeOffset Start = new(2026, 10, 21, 17, 5, 0, TimeSpan.Zero);

    private Mock<IUnfinishedWorkoutRepository> repository = null!;
    private FakeLogger<UnfinishedWorkoutService> logger = null!;
    private UnfinishedWorkoutService service = null!;

    [SetUp]
    public void SetUp()
    {
        repository = new Mock<IUnfinishedWorkoutRepository>(MockBehavior.Strict);
        logger = new FakeLogger<UnfinishedWorkoutService>(new FakeLogCollector());
        service = new UnfinishedWorkoutService(repository.Object, new ExerciseCatalog(), logger);
    }

    [Test]
    public async Task GetSummaryAsync_ReturnsTheStartAndCountsOfExercisesAndStrengthSets()
    {
        repository.Setup(r => r.GetAsync()).ReturnsAsync(Stored());

        var result = await service.GetSummaryAsync();

        Assert.That(result.Value, Is.EqualTo(new UnfinishedSummary(Start, 2, 2)));
    }

    [Test]
    public async Task GetSummaryAsync_NothingStored_ReturnsNoUnfinishedWorkout()
    {
        repository.Setup(r => r.GetAsync()).ReturnsAsync((UnfinishedWorkout?)null);

        Assert.That((await service.GetSummaryAsync()).Value, Is.InstanceOf<NoUnfinishedWorkout>());
    }

    [Test]
    public async Task GetSummaryAsync_ReadFails_ReturnsReadFailedAndLogsIt()
    {
        repository.Setup(r => r.GetAsync()).ThrowsAsync(SQLiteException.New(SQLite3.Result.IOError, "disk I/O error"));

        Assert.That((await service.GetSummaryAsync()).Value, Is.InstanceOf<ReadFailed>());
        Assert.That(LoggedEvents.Of(logger).Select(e => (e.Id, e.Level)), Is.EqualTo(new[] { (CoreEvent.UnfinishedReadFailed, LogLevel.Error) }));
        Assert.That(logger.LatestRecord.Exception, Is.Not.Null);
    }

    [Test]
    public async Task GetAsync_ReturnsTheStoredWorkoutAndLogsCountsOnly()
    {
        var stored = Stored();
        repository.Setup(r => r.GetAsync()).ReturnsAsync(stored);

        var result = await service.GetAsync();

        var read = (UnfinishedWorkout)result.Value!;
        Assert.That((read.StartedAt, read.Note, read.Exercises), Is.EqualTo((stored.StartedAt, stored.Note, stored.Exercises)));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.UnfinishedRecovered, LogLevel.Information, "Unfinished workout recovered with 2 exercises and 2 sets"),
        }));
    }

    [Test]
    public async Task GetAsync_ExerciseNotInTheList_IsLeftOutAndLogged()
    {
        repository.Setup(r => r.GetAsync()).ReturnsAsync(new UnfinishedWorkout(Start, null, [Entry(Press, 1), Entry(9999, 1)]));

        var result = (UnfinishedWorkout)(await service.GetAsync()).Value!;

        Assert.That(result.Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { Press }));
        Assert.That(LoggedEvents.Of(logger).Select(e => e.Id), Is.EqualTo(new[] { CoreEvent.UnfinishedUnknownExercise, CoreEvent.UnfinishedRecovered }));
    }

    [Test]
    public async Task GetAsync_NoExerciseInTheList_ReturnsNoUnfinishedWorkout()
    {
        repository.Setup(r => r.GetAsync()).ReturnsAsync(new UnfinishedWorkout(Start, null, [Entry(9999, 1)]));

        Assert.That((await service.GetAsync()).Value, Is.InstanceOf<NoUnfinishedWorkout>());
    }

    [Test]
    public async Task SaveAsync_StoresTheWorkoutAndLogsCountsOnly()
    {
        var workout = Stored();
        repository.Setup(r => r.ReplaceAsync(workout)).Returns(Task.CompletedTask);

        Assert.That(await service.SaveAsync(workout), Is.EqualTo(UnfinishedWrite.Done));

        repository.Verify(r => r.ReplaceAsync(workout), Times.Once);
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.UnfinishedSaved, LogLevel.Debug, "Unfinished workout stored with 2 exercises and 2 sets"),
        }));
    }

    [Test]
    public async Task SaveAsync_WriteFails_ReturnsFailedAndLogsWithoutContents()
    {
        repository.Setup(r => r.ReplaceAsync(It.IsAny<UnfinishedWorkout>())).ThrowsAsync(SQLiteException.New(SQLite3.Result.Full, "database or disk is full"));
        var workout = new UnfinishedWorkout(Start, Sentinel, [new UnfinishedExercise(Press, Sentinel, false, [new UnfinishedEntry(new Dictionary<string, string?> { ["reps"] = Sentinel })])]);

        Assert.That(await service.SaveAsync(workout), Is.EqualTo(UnfinishedWrite.Failed));

        var records = logger.Collector.GetSnapshot();
        Assert.Multiple(() =>
        {
            Assert.That(records.Select(r => ((CoreEvent)r.Id.Id, r.Level)), Is.EqualTo(new[] { (CoreEvent.UnfinishedSaveFailed, LogLevel.Error) }));
            Assert.That(records.Single().Message, Is.EqualTo("Storing the unfinished workout with 1 exercises failed"));
            Assert.That(records.Single().Exception, Is.Not.Null);
        });
    }

    [Test]
    public void SaveAsync_Null_Throws() =>
        Assert.That(() => service.SaveAsync(null!), Throws.ArgumentNullException);

    [Test]
    public async Task DiscardAsync_DeletesTheWorkoutAndLogsIt()
    {
        repository.Setup(r => r.DeleteAsync()).Returns(Task.CompletedTask);

        Assert.That(await service.DiscardAsync(), Is.EqualTo(UnfinishedWrite.Done));

        repository.Verify(r => r.DeleteAsync(), Times.Once);
        Assert.That(LoggedEvents.Of(logger).Select(e => e.Id), Is.EqualTo(new[] { CoreEvent.UnfinishedDiscarded }));
    }

    [Test]
    public async Task DiscardAsync_WriteFails_ReturnsFailedAndLogsIt()
    {
        repository.Setup(r => r.DeleteAsync()).ThrowsAsync(SQLiteException.New(SQLite3.Result.IOError, "disk I/O error"));

        Assert.That(await service.DiscardAsync(), Is.EqualTo(UnfinishedWrite.Failed));

        Assert.That(LoggedEvents.Of(logger).Select(e => (e.Id, e.Level)), Is.EqualTo(new[] { (CoreEvent.UnfinishedDiscardFailed, LogLevel.Error) }));
    }

    /// <summary>A press with two sets and a treadmill entry, which is not a set.</summary>
    private static UnfinishedWorkout Stored() => new(Start, "note", [Entry(Press, 2), Entry(Treadmill, 1)]);

    private static UnfinishedExercise Entry(int exerciseId, int entries) =>
        new(exerciseId, null, false, [.. Enumerable.Range(0, entries).Select(_ => new UnfinishedEntry(new Dictionary<string, string?>()))]);
}
