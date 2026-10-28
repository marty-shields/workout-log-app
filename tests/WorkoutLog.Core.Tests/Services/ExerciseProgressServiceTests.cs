using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;

namespace WorkoutLog.Core.Tests.Services;

public class ExerciseProgressServiceTests
{
    /// <summary>Barbell Back Squat: lift.</summary>
    private const int Squat = 6;

    /// <summary>Plate Front Raise: lift, equipment Other.</summary>
    private const int PlateFrontRaise = 259;

    /// <summary>Push-up: bodyweight.</summary>
    private const int PushUp = 264;

    /// <summary>Outdoor Run: cardio.</summary>
    private const int Run = 311;

    private static readonly DateTimeOffset Now = new(2026, 10, 28, 9, 0, 0, TimeSpan.Zero);
    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();

    private Mock<IWorkoutRepository> repository = null!;
    private FakeLogger<ExerciseProgressService> logger = null!;
    private ExerciseProgressService service = null!;

    [SetUp]
    public void SetUp()
    {
        repository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        logger = new FakeLogger<ExerciseProgressService>();
        var clock = new FakeTimeProvider(Now);
        clock.SetLocalTimeZone(TimeZoneInfo.Utc);
        service = new ExerciseProgressService(repository.Object, Catalog, new RangeFilter(clock), new TopSetRule(), new E1rmRule(), new RecordsRule(), logger);
    }

    [Test]
    public void Exercises_AreTheOnesNamed() =>
        Assert.That(new[] { Squat, PlateFrontRaise, PushUp, Run }.Select(id => Catalog.All.Single(e => e.Id == id).Name),
            Is.EqualTo(new[] { "Barbell Back Squat", "Plate Front Raise", "Push-up", "Outdoor Run" }));

    [Test]
    public async Task LoadAsync_ReturnsEachSessionWithItsSets()
    {
        var older = Now.AddDays(-7);
        repository.Setup(r => r.GetSessionsAsync(Squat)).ReturnsAsync(
        [
            new WorkoutSession(1, older, [new StrengthExercise(Squat, [Set(8, 60m)])]),
            new WorkoutSession(2, Now, [new StrengthExercise(Squat, [Set(5, 65m), Set(3, 70m)])]),
        ]);

        var result = await service.LoadAsync(Squat);

        Assert.That(result, Is.EqualTo(new ExerciseProgressResult(
        [
            new LiftSession(1, older, [Set(8, 60m)]),
            new LiftSession(2, Now, [Set(5, 65m), Set(3, 70m)]),
        ], Failed: false)).UsingPropertiesComparer());
    }

    [Test]
    public async Task LoadAsync_LiftTwiceInAWorkout_IsOneSessionWithTheSetsInWorkoutOrder()
    {
        repository.Setup(r => r.GetSessionsAsync(Squat)).ReturnsAsync(
            [new WorkoutSession(1, Now, [new StrengthExercise(Squat, [Set(5, 60m)]), new StrengthExercise(Squat, [Set(3, 80m), Set(2, 85m)])])]);

        var result = await service.LoadAsync(Squat);

        Assert.That(result.Sessions, Is.EqualTo(new[] { new LiftSession(1, Now, [Set(5, 60m), Set(3, 80m), Set(2, 85m)]) }).UsingPropertiesComparer());
    }

    [Test]
    public async Task LoadAsync_LiftWithEquipmentOther_Loads()
    {
        repository.Setup(r => r.GetSessionsAsync(PlateFrontRaise)).ReturnsAsync([]);

        Assert.That((await service.LoadAsync(PlateFrontRaise)).Failed, Is.False);
    }

    [Test]
    public async Task LoadAsync_LogsTheCountAtInformation()
    {
        repository.Setup(r => r.GetSessionsAsync(Squat)).ReturnsAsync(
            [new WorkoutSession(1, Now, [new StrengthExercise(Squat, [Set(5, 60m)])]), new WorkoutSession(2, Now.AddDays(1), [])]);

        await service.LoadAsync(Squat);

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.ExerciseProgressLoaded, LogLevel.Information, "Loaded 2 sessions of exercise 6") }));
    }

    [Test]
    public async Task LoadAsync_UnknownExercise_FailsAndLogsAWarningWithoutReading()
    {
        var result = await service.LoadAsync(999);

        Assert.That(result, Is.EqualTo(new ExerciseProgressResult([], Failed: true)).UsingPropertiesComparer());
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.ExerciseProgressUnknownExercise, LogLevel.Warning, "Exercise 999 is not in the exercise list") }));
        repository.VerifyNoOtherCalls();
    }

    [TestCase(PushUp)]
    [TestCase(Run)]
    public async Task LoadAsync_NotALift_FailsAndLogsAWarningWithoutReading(int exerciseId)
    {
        var result = await service.LoadAsync(exerciseId);

        Assert.That(result, Is.EqualTo(new ExerciseProgressResult([], Failed: true)).UsingPropertiesComparer());
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.ExerciseProgressNotALift, LogLevel.Warning, $"Exercise {exerciseId} is not a lift") }));
        repository.VerifyNoOtherCalls();
    }

    [Test]
    public async Task LoadAsync_ReadFails_ReturnsFailedAndLogsAnErrorWithTheException()
    {
        var error = new InvalidOperationException("disk");
        repository.Setup(r => r.GetSessionsAsync(Squat)).ThrowsAsync(error);

        var result = await service.LoadAsync(Squat);

        Assert.That(result, Is.EqualTo(new ExerciseProgressResult([], Failed: true)).UsingPropertiesComparer());
        Assert.Multiple(() =>
        {
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.ExerciseProgressLoadFailed, LogLevel.Error, "Loading the sessions of exercise 6 failed") }));
            Assert.That(logger.Collector.LatestRecord.Exception, Is.SameAs(error));
        });
    }

    [Test]
    public void Summarise_KeepsTheSessionsInTheRangeOldestFirstWithTheirValuesAndRecords()
    {
        var outOfRange = new LiftSession(1, Now.AddMonths(-4), [Set(1, 200m)]);
        var noWeight = new LiftSession(2, Now.AddDays(-14), [Set(8, null)]);
        var heavy = new LiftSession(3, Now, [Set(8, 62.5m), Set(3, 70m)]);
        var light = new LiftSession(4, Now.AddDays(-7), [Set(15, 50m), Set(5, 60m)]);

        var progress = service.Summarise([outOfRange, noWeight, heavy, light], ChartRange.ThreeMonths);

        Assert.That(progress, Is.EqualTo(new LiftProgress(
        [
            new LiftSessionProgress(noWeight, null, null),
            new LiftSessionProgress(light, new ExerciseBest(60m, 5, null, null), 60m * (1 + 5 / 30m)),
            new LiftSessionProgress(heavy, new ExerciseBest(70m, 3, null, null), 62.5m * (1 + 8 / 30m)),
        ], new LiftRecords(
            new ProgressRecord<ExerciseBest>(new ExerciseBest(70m, 3, null, null), Now),
            new ProgressRecord<decimal>(62.5m * (1 + 8 / 30m), Now)))).UsingPropertiesComparer());
    }

    [Test]
    public void Summarise_All_KeepsEverySession()
    {
        var old = new LiftSession(1, Now.AddYears(-3), [Set(1, 200m)]);

        var progress = service.Summarise([old], ChartRange.All);

        Assert.That(progress.Records.HeaviestWeight, Is.EqualTo(new ProgressRecord<ExerciseBest>(new ExerciseBest(200m, 1, null, null), old.StartedAt)));
    }

    [Test]
    public void Summarise_NoSessionsInTheRange_HasNoSessionsOrRecords() =>
        Assert.That(service.Summarise([new LiftSession(1, Now.AddYears(-2), [Set(5, 60m)])], ChartRange.Year),
            Is.EqualTo(new LiftProgress([], new LiftRecords(null, null))).UsingPropertiesComparer());

    private static StrengthSet Set(int? reps, decimal? weightKg) => new(reps, weightKg, null, null);
}
