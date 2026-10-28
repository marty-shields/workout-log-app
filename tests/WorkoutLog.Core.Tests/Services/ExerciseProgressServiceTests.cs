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

    /// <summary>Plank: hold.</summary>
    private const int Plank = 258;

    /// <summary>Outdoor Run: cardio.</summary>
    private const int Run = 311;

    /// <summary>Machine Assisted Pull-up: assisted.</summary>
    private const int AssistedPullUp = 218;

    /// <summary>Farmer's Walk: carry.</summary>
    private const int FarmersWalk = 187;

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
        service = new ExerciseProgressService(repository.Object, Catalog, new RangeFilter(clock), new TopSetRule(), new E1rmRule(), new RecordsRule(),
            new BodyweightRule(new TopSetRule(), new E1rmRule()), new BodyweightRecordsRule(new RecordsRule()),
            new LeastAssistRule(), new AssistedRecordsRule(), new HoldRule(), new HoldRecordsRule(), new CarryRule(), new CarryRecordsRule(), logger);
    }

    [Test]
    public void Exercises_AreTheOnesNamed() =>
        Assert.That(new[] { Squat, PlateFrontRaise, PushUp, Plank, Run, AssistedPullUp, FarmersWalk }.Select(id => Catalog.All.Single(e => e.Id == id).Name),
            Is.EqualTo(new[] { "Barbell Back Squat", "Plate Front Raise", "Push-up", "Plank", "Outdoor Run", "Machine Assisted Pull-up", "Farmer's Walk" }));

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
            new StrengthSession(1, older, [Set(8, 60m)]),
            new StrengthSession(2, Now, [Set(5, 65m), Set(3, 70m)]),
        ], Failed: false)).UsingPropertiesComparer());
    }

    [Test]
    public async Task LoadAsync_LiftTwiceInAWorkout_IsOneSessionWithTheSetsInWorkoutOrder()
    {
        repository.Setup(r => r.GetSessionsAsync(Squat)).ReturnsAsync(
            [new WorkoutSession(1, Now, [new StrengthExercise(Squat, [Set(5, 60m)]), new StrengthExercise(Squat, [Set(3, 80m), Set(2, 85m)])])]);

        var result = await service.LoadAsync(Squat);

        Assert.That(result.Sessions, Is.EqualTo(new[] { new StrengthSession(1, Now, [Set(5, 60m), Set(3, 80m), Set(2, 85m)]) }).UsingPropertiesComparer());
    }

    [Test]
    public async Task LoadAsync_LiftWithEquipmentOther_Loads()
    {
        repository.Setup(r => r.GetSessionsAsync(PlateFrontRaise)).ReturnsAsync([]);

        Assert.That((await service.LoadAsync(PlateFrontRaise)).Failed, Is.False);
    }

    [Test]
    public async Task LoadAsync_Bodyweight_ReturnsEachSessionWithItsSets()
    {
        repository.Setup(r => r.GetSessionsAsync(PushUp)).ReturnsAsync([new WorkoutSession(1, Now, [new StrengthExercise(PushUp, [Set(20, null), Set(8, 10m)])])]);

        var result = await service.LoadAsync(PushUp);

        Assert.That(result, Is.EqualTo(new ExerciseProgressResult([new StrengthSession(1, Now, [Set(20, null), Set(8, 10m)])], Failed: false))
            .UsingPropertiesComparer());
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

    [TestCase(AssistedPullUp)]
    [TestCase(Plank)]
    [TestCase(FarmersWalk)]
    public async Task LoadAsync_AssistedHoldOrCarry_ReturnsItsSessions(int exerciseId)
    {
        repository.Setup(r => r.GetSessionsAsync(exerciseId)).ReturnsAsync(
            [new WorkoutSession(1, Now, [new StrengthExercise(exerciseId, [Set(8, 10m)])])]);

        var result = await service.LoadAsync(exerciseId);

        Assert.That(result, Is.EqualTo(new ExerciseProgressResult([new StrengthSession(1, Now, [Set(8, 10m)])], Failed: false)).UsingPropertiesComparer());
    }

    [TestCase(Run)]
    public async Task LoadAsync_NoExercisePage_FailsAndLogsAWarningWithoutReading(int exerciseId)
    {
        var result = await service.LoadAsync(exerciseId);

        Assert.That(result, Is.EqualTo(new ExerciseProgressResult([], Failed: true)).UsingPropertiesComparer());
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.ExerciseProgressNotSupported, LogLevel.Warning, $"Exercise {exerciseId} has no progress page") }));
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
    public void SummariseLift_KeepsTheSessionsInTheRangeOldestFirstWithTheirValuesAndRecords()
    {
        var outOfRange = new StrengthSession(1, Now.AddMonths(-4), [Set(1, 200m)]);
        var noWeight = new StrengthSession(2, Now.AddDays(-14), [Set(8, null)]);
        var heavy = new StrengthSession(3, Now, [Set(8, 62.5m), Set(3, 70m)]);
        var light = new StrengthSession(4, Now.AddDays(-7), [Set(15, 50m), Set(5, 60m)]);

        var progress = service.SummariseLift([outOfRange, noWeight, heavy, light], ChartRange.ThreeMonths);

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
    public void SummariseLift_All_KeepsEverySession()
    {
        var old = new StrengthSession(1, Now.AddYears(-3), [Set(1, 200m)]);

        var progress = service.SummariseLift([old], ChartRange.All);

        Assert.That(progress.Records.HeaviestWeight, Is.EqualTo(new ProgressRecord<ExerciseBest>(new ExerciseBest(200m, 1, null, null), old.StartedAt)));
    }

    [Test]
    public void SummariseLift_NoSessionsInTheRange_HasNoSessionsOrRecords() =>
        Assert.That(service.SummariseLift([new StrengthSession(1, Now.AddYears(-2), [Set(5, 60m)])], ChartRange.Year),
            Is.EqualTo(new LiftProgress([], new LiftRecords(null, null))).UsingPropertiesComparer());

    [Test]
    public void SummariseBodyweight_KeepsTheSessionsInTheRangeOldestFirstWithTheirValuesAndRecords()
    {
        var outOfRange = new StrengthSession(1, Now.AddMonths(-4), [Set(50, null)]);
        var mixed = new StrengthSession(2, Now, [Set(20, null), Set(18, 0m), Set(8, 10m)]);
        var unweighted = new StrengthSession(3, Now.AddDays(-7), [Set(25, null), Set(15, null)]);

        var progress = service.SummariseBodyweight([outOfRange, mixed, unweighted], ChartRange.ThreeMonths);

        Assert.That(progress, Is.EqualTo(new BodyweightProgress(
        [
            new BodyweightSessionProgress(unweighted, 25, 40, null, null),
            new BodyweightSessionProgress(mixed, 20, 38, new ExerciseBest(10m, 8, null, null), 10m * (1 + 8 / 30m)),
        ], new BodyweightRecords(
            new ProgressRecord<int>(25, unweighted.StartedAt),
            new ProgressRecord<int>(40, unweighted.StartedAt),
            new ProgressRecord<ExerciseBest>(new ExerciseBest(10m, 8, null, null), Now),
            new ProgressRecord<decimal>(10m * (1 + 8 / 30m), Now)))).UsingPropertiesComparer());
    }

    [Test]
    public void SummariseBodyweight_NoSessionsInTheRange_HasNoSessionsOrRecords() =>
        Assert.That(service.SummariseBodyweight([new StrengthSession(1, Now.AddYears(-2), [Set(20, null)])], ChartRange.Year),
            Is.EqualTo(new BodyweightProgress([], new BodyweightRecords(null, null, null, null))).UsingPropertiesComparer());

    [Test]
    public void SummariseAssisted_KeepsTheSessionsInTheRangeOldestFirstWithTheirLeastAssistAndRecords()
    {
        var outOfRange = new StrengthSession(1, Now.AddMonths(-4), [Set(10, null)]);
        var latest = new StrengthSession(2, Now, [Set(8, 5m), Set(5, null)]);
        var earlier = new StrengthSession(3, Now.AddDays(-7), [Set(8, 10m), Set(6, 5m)]);

        var progress = service.SummariseAssisted([outOfRange, latest, earlier], ChartRange.ThreeMonths);

        Assert.That(progress, Is.EqualTo(new AssistedProgress(
        [
            new AssistedSessionProgress(earlier, new ExerciseBest(5m, 6, null, null)),
            new AssistedSessionProgress(latest, new ExerciseBest(0m, 5, null, null)),
        ], new AssistedRecords(new ProgressRecord<ExerciseBest>(new ExerciseBest(0m, 5, null, null), Now)))).UsingPropertiesComparer());
    }

    [Test]
    public void SummariseHold_KeepsTheSessionsInTheRangeOldestFirstWithTheirValuesAndRecords()
    {
        var outOfRange = new StrengthSession(1, Now.AddMonths(-4), [Timed(600, 50m)]);
        var latest = new StrengthSession(2, Now, [Timed(105, null), Timed(60, 10m), Timed(80, 0m)]);
        var earlier = new StrengthSession(3, Now.AddDays(-7), [Timed(90, null)]);

        var progress = service.SummariseHold([outOfRange, latest, earlier], ChartRange.ThreeMonths);

        var heaviest = new ExerciseBest(10m, null, TimeSpan.FromSeconds(60), null);
        Assert.That(progress, Is.EqualTo(new HoldProgress(
        [
            new HoldSessionProgress(earlier, TimeSpan.FromSeconds(90), null),
            new HoldSessionProgress(latest, TimeSpan.FromSeconds(105), heaviest),
        ], new HoldRecords(new ProgressRecord<TimeSpan>(TimeSpan.FromSeconds(105), Now), new ProgressRecord<ExerciseBest>(heaviest, Now)))).UsingPropertiesComparer());
    }

    [Test]
    public void SummariseCarry_KeepsTheSessionsInTheRangeOldestFirstWithTheirValuesAndRecords()
    {
        var outOfRange = new StrengthSession(1, Now.AddMonths(-4), [Carried(100m, 500m)]);
        var latest = new StrengthSession(2, Now, [Carried(28m, 40m), Carried(32m, 40m)]);
        var earlier = new StrengthSession(3, Now.AddDays(-7), [Carried(null, 60m)]);

        var progress = service.SummariseCarry([outOfRange, latest, earlier], ChartRange.ThreeMonths);

        Assert.That(progress, Is.EqualTo(new CarryProgress(
        [
            new CarrySessionProgress(earlier, null, new ExerciseBest(null, null, null, 60m)),
            new CarrySessionProgress(latest, new ExerciseBest(32m, null, null, 40m), new ExerciseBest(32m, null, null, 40m)),
        ], new CarryRecords(
            new ProgressRecord<ExerciseBest>(new ExerciseBest(32m, null, null, 40m), Now),
            new ProgressRecord<ExerciseBest>(new ExerciseBest(null, null, null, 60m), Now.AddDays(-7))))).UsingPropertiesComparer());
    }

    private static StrengthSet Set(int? reps, decimal? weightKg) => new(reps, weightKg, null, null);

    private static StrengthSet Timed(int seconds, decimal? weightKg) => new(null, weightKg, TimeSpan.FromSeconds(seconds), null);

    private static StrengthSet Carried(decimal? weightKg, decimal metres) => new(null, weightKg, null, metres);
}
