using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;

namespace WorkoutLog.Core.Tests.Services;

public class CardioProgressServiceTests
{
    /// <summary>Barbell Back Squat: lift.</summary>
    private const int Squat = 6;

    /// <summary>Plank: hold.</summary>
    private const int Plank = 258;

    /// <summary>Stair Climber: cardio, no distance.</summary>
    private const int StairClimber = 309;

    /// <summary>Outdoor Run: cardio.</summary>
    private const int Run = 311;

    private static readonly DateTimeOffset Now = new(2026, 10, 28, 9, 0, 0, TimeSpan.Zero);
    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();

    private Mock<IWorkoutRepository> repository = null!;
    private FakeLogger<CardioProgressService> logger = null!;
    private CardioProgressService service = null!;

    [SetUp]
    public void SetUp()
    {
        repository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        logger = new FakeLogger<CardioProgressService>();
        var clock = new FakeTimeProvider(Now);
        clock.SetLocalTimeZone(TimeZoneInfo.Utc);
        service = new CardioProgressService(repository.Object, Catalog, new RangeFilter(clock), new CardioSessionRule(), new CardioRecordsRule(), logger);
    }

    [Test]
    public void Exercises_AreTheOnesNamed() =>
        Assert.That(new[] { Squat, Plank, StairClimber, Run }.Select(id => Catalog.All.Single(e => e.Id == id).Name),
            Is.EqualTo(new[] { "Barbell Back Squat", "Plank", "Stair Climber", "Outdoor Run" }));

    [Test]
    public async Task LoadAsync_ReturnsEachSessionWithItsEntry()
    {
        var older = Now.AddDays(-7);
        repository.Setup(r => r.GetSessionsAsync(Run)).ReturnsAsync(
        [
            new WorkoutSession(1, older, [new CardioExercise(Run, Entry(TimeSpan.FromMinutes(25), 4m))]),
            new WorkoutSession(2, Now, [new CardioExercise(Run, Entry(TimeSpan.FromMinutes(30), 5.2m))]),
        ]);

        var result = await service.LoadAsync(Run);

        Assert.That(result, Is.EqualTo(new CardioProgressResult(
        [
            new CardioSession(1, older, [Entry(TimeSpan.FromMinutes(25), 4m)]),
            new CardioSession(2, Now, [Entry(TimeSpan.FromMinutes(30), 5.2m)]),
        ], Failed: false)).UsingPropertiesComparer());
    }

    [Test]
    public async Task LoadAsync_TwiceInAWorkout_IsOneSessionWithTheEntriesInWorkoutOrder()
    {
        repository.Setup(r => r.GetSessionsAsync(Run)).ReturnsAsync(
            [new WorkoutSession(1, Now, [new CardioExercise(Run, Entry(TimeSpan.FromMinutes(18), 3m)), new CardioExercise(Run, Entry(TimeSpan.FromMinutes(30), 5.2m))])]);

        var result = await service.LoadAsync(Run);

        Assert.That(result.Sessions, Is.EqualTo(new[] { new CardioSession(1, Now, [Entry(TimeSpan.FromMinutes(18), 3m), Entry(TimeSpan.FromMinutes(30), 5.2m)]) })
            .UsingPropertiesComparer());
    }

    [Test]
    public async Task LoadAsync_CardioWithNoDistance_Loads()
    {
        repository.Setup(r => r.GetSessionsAsync(StairClimber)).ReturnsAsync([]);

        Assert.That((await service.LoadAsync(StairClimber)).Failed, Is.False);
    }

    [Test]
    public async Task LoadAsync_LogsTheCountAtInformation()
    {
        repository.Setup(r => r.GetSessionsAsync(Run)).ReturnsAsync(
            [new WorkoutSession(1, Now, [new CardioExercise(Run, Entry(TimeSpan.FromMinutes(30), 5m))]), new WorkoutSession(2, Now.AddDays(1), [])]);

        await service.LoadAsync(Run);

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.CardioProgressLoaded, LogLevel.Information, "Loaded 2 sessions of exercise 311") }));
    }

    [Test]
    public async Task LoadAsync_UnknownExercise_FailsAndLogsAWarningWithoutReading()
    {
        var result = await service.LoadAsync(999);

        Assert.That(result, Is.EqualTo(new CardioProgressResult([], Failed: true)).UsingPropertiesComparer());
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.CardioProgressUnknownExercise, LogLevel.Warning, "Exercise 999 is not in the exercise list") }));
        repository.VerifyNoOtherCalls();
    }

    [TestCase(Squat)]
    [TestCase(Plank)]
    public async Task LoadAsync_NotCardio_FailsAndLogsAWarningWithoutReading(int exerciseId)
    {
        var result = await service.LoadAsync(exerciseId);

        Assert.That(result, Is.EqualTo(new CardioProgressResult([], Failed: true)).UsingPropertiesComparer());
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.CardioProgressNotCardio, LogLevel.Warning, $"Exercise {exerciseId} is not cardio") }));
        repository.VerifyNoOtherCalls();
    }

    [Test]
    public async Task LoadAsync_ReadFails_ReturnsFailedAndLogsAnErrorWithTheException()
    {
        var error = new InvalidOperationException("disk");
        repository.Setup(r => r.GetSessionsAsync(Run)).ThrowsAsync(error);

        var result = await service.LoadAsync(Run);

        Assert.That(result, Is.EqualTo(new CardioProgressResult([], Failed: true)).UsingPropertiesComparer());
        Assert.Multiple(() =>
        {
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.CardioProgressLoadFailed, LogLevel.Error, "Loading the sessions of exercise 311 failed") }));
            Assert.That(logger.Collector.LatestRecord.Exception, Is.SameAs(error));
        });
    }

    [Test]
    public void Summarise_KeepsTheSessionsInTheRangeOldestFirstWithTheirBestAndRecords()
    {
        var outOfRange = new CardioSession(1, Now.AddMonths(-4), [Entry(TimeSpan.FromHours(2), 20m)]);
        var twice = new CardioSession(2, Now, [Entry(TimeSpan.FromMinutes(18), 3m), Entry(TimeSpan.FromMinutes(30), 5.2m)]);
        var older = new CardioSession(3, Now.AddDays(-7), [Entry(TimeSpan.FromMinutes(40), 5m)]);

        var progress = service.Summarise([outOfRange, twice, older], ChartRange.ThreeMonths);

        Assert.That(progress, Is.EqualTo(new CardioProgress(
        [
            new CardioSessionProgress(older, new CardioBest(5m, TimeSpan.FromMinutes(40), null, null, null)),
            new CardioSessionProgress(twice, new CardioBest(5.2m, TimeSpan.FromMinutes(30), null, null, null)),
        ], new CardioRecords(
            new ProgressRecord<decimal>(5.2m, Now),
            new ProgressRecord<TimeSpan>(TimeSpan.FromMinutes(40), older.StartedAt),
            null,
            null,
            null))).UsingPropertiesComparer());
    }

    [Test]
    public void Summarise_All_KeepsEverySession()
    {
        var old = new CardioSession(1, Now.AddYears(-3), [Entry(TimeSpan.FromHours(2), 20m)]);

        var progress = service.Summarise([old], ChartRange.All);

        Assert.That(progress.Records.LongestDistance, Is.EqualTo(new ProgressRecord<decimal>(20m, old.StartedAt)));
    }

    [Test]
    public void Summarise_NoSessionsInTheRange_HasNoSessionsOrRecords() =>
        Assert.That(service.Summarise([new CardioSession(1, Now.AddYears(-2), [Entry(TimeSpan.FromMinutes(30), 5m)])], ChartRange.Year),
            Is.EqualTo(new CardioProgress([], new CardioRecords(null, null, null, null, null))).UsingPropertiesComparer());

    private static CardioEntry Entry(TimeSpan duration, decimal distance) => new(duration, distance, null, null, null, null, null, null);
}
