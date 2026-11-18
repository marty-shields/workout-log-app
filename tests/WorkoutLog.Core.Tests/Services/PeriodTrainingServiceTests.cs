using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;
using WorkoutLog.Core.Tests.Domain.Workouts.Consistency;
using static WorkoutLog.Core.Tests.Domain.Exercises.Bests.BestRuleCases;

namespace WorkoutLog.Core.Tests.Services;

public class PeriodTrainingServiceTests
{
    private const int UnknownId = 9999;

    /// <summary>Sun 15 Nov 2026, 12:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 11, 15, 12, 0, 0, TimeSpan.Zero);

    private static readonly TimeZoneInfo Plus10 = ConsistencyZones.Plus10;

    private static readonly Period October = Period.Containing(PeriodKind.Month, new DateOnly(2026, 10, 15));

    /// <summary>00:00 on Thu 1 Oct in UTC+10: the month's first instant.</summary>
    private static readonly DateTimeOffset OctoberStart = new(2026, 10, 1, 0, 0, 0, TimeSpan.FromHours(10));

    /// <summary>00:00 on Sun 1 Nov in UTC+10: the first instant after the month.</summary>
    private static readonly DateTimeOffset OctoberEnd = new(2026, 11, 1, 0, 0, 0, TimeSpan.FromHours(10));

    private static readonly DateTimeOffset LongAgo = new(2026, 1, 5, 12, 0, 0, TimeSpan.Zero);

    private Mock<IWorkoutRepository> repository = null!;
    private FakeLogger<PeriodTrainingService> logger = null!;
    private PeriodTrainingService service = null!;
    private int nextId;

    [SetUp]
    public void SetUp()
    {
        repository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        logger = new FakeLogger<PeriodTrainingService>();
        UseClock(Now, Plus10);
        nextId = 0;
    }

    [Test]
    public async Task GetRecordsAsync_NothingSaved_IsEmpty()
    {
        Setup();

        var result = (await service.GetRecordsAsync(October)).Loaded();

        Assert.Multiple(() =>
        {
            Assert.That(result.Records, Is.Empty);
            Assert.That(result.PerMonth, Is.Empty);
        });
    }

    [Test]
    public async Task GetRecordsAsync_NoRecordInThePeriod_IsEmptyEvenWithOlderOnes()
    {
        Setup(Workout(LongAgo, Sets(Squat, Set(5, 60m))), Workout(LongAgo.AddDays(1), Sets(Squat, Set(5, 70m))));

        Assert.That((await service.GetRecordsAsync(October)).Loaded().Records, Is.Empty);
    }

    [Test]
    public async Task GetRecordsAsync_FirstAndLastMomentOfThePeriodInThePhonesZone_AreIn()
    {
        Setup(
            Workout(LongAgo, Sets(Squat, Set(5, 60m)), Sets(Curl, Set(10, 10m))),
            Workout(OctoberStart, Sets(Squat, Set(5, 70m))),
            Workout(OctoberEnd.AddTicks(-1), Sets(Curl, Set(10, 12m))));

        var records = (await service.GetRecordsAsync(October)).Loaded().Records;

        Assert.That(records.Select(r => (r.ExerciseId, r.StartedAt)), Is.EqualTo(new[] { (Curl, OctoberEnd.AddTicks(-1)), (Squat, OctoberStart) }));
    }

    [Test]
    public async Task GetRecordsAsync_JustOutsideThePeriodInThePhonesZone_IsOut()
    {
        Setup(
            Workout(LongAgo, Sets(Squat, Set(5, 60m)), Sets(Curl, Set(10, 10m))),
            Workout(OctoberStart.AddTicks(-1), Sets(Squat, Set(5, 70m))),
            Workout(OctoberEnd, Sets(Curl, Set(10, 12m))));

        Assert.That((await service.GetRecordsAsync(October)).Loaded().Records, Is.Empty);
    }

    [Test]
    public async Task GetRecordsAsync_AfterNow_IsOut()
    {
        var period = Period.Containing(PeriodKind.Month, new DateOnly(2026, 11, 15));
        Setup(Workout(LongAgo, Sets(Squat, Set(5, 60m))), Workout(Now.AddMinutes(1), Sets(Squat, Set(5, 70m))), Workout(Now, Sets(Curl, Set(5, 10m))));

        Assert.That((await service.GetRecordsAsync(period)).Loaded().Records, Is.Empty);
    }

    [Test]
    public async Task GetRecordsAsync_ADstZone_PutsAWorkoutOnItsLocalDay()
    {
        UseClock(Now, ConsistencyZones.ForwardOn25Oct);
        var inOctober = new DateTimeOffset(2026, 10, 31, 22, 30, 0, TimeSpan.Zero);
        var inNovember = new DateTimeOffset(2026, 10, 31, 22, 0, 0, TimeSpan.Zero);
        Setup(
            Workout(LongAgo, Sets(Squat, Set(5, 60m)), Sets(Curl, Set(10, 10m))),
            Workout(inNovember, Sets(Squat, Set(5, 70m))),
            Workout(inOctober.AddDays(-20), Sets(Curl, Set(10, 12m))));

        var records = (await service.GetRecordsAsync(October)).Loaded().Records;

        Assert.That(records.Select(r => r.ExerciseId), Is.EqualTo(new[] { Curl }));
    }

    [Test]
    public async Task GetRecordsAsync_EveryRecordKind_IsFound()
    {
        Func<int, TimeSpan> mins = m => TimeSpan.FromMinutes(m);
        Setup(
            Workout(LongAgo,
                Sets(Squat, Set(5, 60m)), Sets(PushUp, Set(10, 0m)), Sets(AssistedPullUp, Set(10, 30m)), Sets(Plank, Set(null, null, mins(1))),
                Sets(FarmersWalk, Set(null, 20m, mins(1), 20m)), Entry(Run, mins(30), 5m)),
            Workout(OctoberStart.AddDays(1),
                Sets(Squat, Set(5, 70m)), Sets(PushUp, Set(15, 0m)), Sets(AssistedPullUp, Set(10, 20m)), Sets(Plank, Set(null, null, mins(2))),
                Sets(FarmersWalk, Set(null, 30m, mins(1), 40m)), Entry(Run, mins(30), 6m)));

        var records = (await service.GetRecordsAsync(October)).Loaded().Records;

        Assert.That(records.Select(r => r.ExerciseId), Is.EquivalentTo(new[] { Squat, PushUp, AssistedPullUp, Plank, FarmersWalk, Run }));
    }

    [Test]
    public async Task GetRecordsAsync_Tie_IsNotARecord()
    {
        Setup(Workout(LongAgo, Sets(Squat, Set(5, 60m))), Workout(OctoberStart.AddDays(1), Sets(Squat, Set(5, 60m))));

        Assert.That((await service.GetRecordsAsync(October)).Loaded().Records, Is.Empty);
    }

    [Test]
    public async Task GetRecordsAsync_ManyRecordsOfAnExercise_CountsEach()
    {
        Setup(
            Workout(LongAgo, Sets(Squat, Set(5, 60m))),
            Workout(OctoberStart.AddDays(1), Sets(Squat, Set(5, 70m))),
            Workout(OctoberStart.AddDays(5), Sets(Squat, Set(5, 75m))));

        var records = (await service.GetRecordsAsync(October)).Loaded().Records;

        Assert.Multiple(() =>
        {
            Assert.That(records.Select(r => r.StartedAt), Is.EqualTo(new[] { OctoberStart.AddDays(5), OctoberStart.AddDays(1) }));
            Assert.That(records[0].Best, Is.EqualTo(new ExerciseBest(75m, 5, null, null)));
        });
    }

    [Test]
    public async Task GetRecordsAsync_MoreThanFive_ReturnsAllNewestFirstSameStartAToZ()
    {
        var same = OctoberStart.AddDays(3);
        Setup(
            Workout(LongAgo, Sets(Squat, Set(5, 60m)), Sets(Curl, Set(10, 10m)), Sets(PushUp, Set(10, 0m)), Sets(Plank, Set(null, null, TimeSpan.FromMinutes(1))),
                Sets(AssistedPullUp, Set(10, 30m)), Sets(FarmersWalk, Set(null, 20m, TimeSpan.FromMinutes(1), 20m)), Entry(Run, TimeSpan.FromMinutes(30), 5m)),
            Workout(same,
                Sets(Squat, Set(5, 70m)), Sets(Curl, Set(10, 12m)), Sets(PushUp, Set(15, 0m)), Sets(Plank, Set(null, null, TimeSpan.FromMinutes(2))),
                Sets(AssistedPullUp, Set(10, 20m)), Sets(FarmersWalk, Set(null, 30m, TimeSpan.FromMinutes(1), 40m))),
            Workout(same.AddDays(2), Entry(Run, TimeSpan.FromMinutes(30), 6m)));

        var records = (await service.GetRecordsAsync(October)).Loaded().Records;

        var sameStart = new[] { Squat, Curl, PushUp, Plank, AssistedPullUp, FarmersWalk }.OrderBy(id => Definition(id).Name, StringComparer.OrdinalIgnoreCase);
        Assert.That(records.Select(r => r.ExerciseId), Is.EqualTo(sameStart.Prepend(Run)));
    }

    [Test]
    public async Task GetRecordsAsync_MonthOrWeek_HasNoMonthCounts()
    {
        Setup(Workout(LongAgo, Sets(Squat, Set(5, 60m))), Workout(OctoberStart.AddDays(1), Sets(Squat, Set(5, 70m))));

        Assert.Multiple(async () =>
        {
            Assert.That((await service.GetRecordsAsync(October)).Loaded().PerMonth, Is.Empty);
            Assert.That((await service.GetRecordsAsync(Period.Containing(PeriodKind.Week, new DateOnly(2026, 10, 2)))).Loaded().PerMonth, Is.Empty);
        });
    }

    [Test]
    public async Task GetRecordsAsync_Year_CountsTheRecordsOfEachLocalMonth()
    {
        var jan31Late = new DateTimeOffset(2026, 1, 31, 15, 0, 0, TimeSpan.Zero);
        Setup(
            Workout(LongAgo.AddYears(-1), Sets(Squat, Set(5, 50m)), Sets(Curl, Set(10, 8m))),
            Workout(jan31Late, Sets(Squat, Set(5, 60m))),
            Workout(OctoberStart, Sets(Squat, Set(5, 70m)), Sets(Curl, Set(10, 10m))),
            Workout(OctoberEnd.AddTicks(-1), Sets(Squat, Set(5, 80m))),
            Workout(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.FromHours(10)), Sets(Squat, Set(5, 90m))));

        var result = (await service.GetRecordsAsync(Period.Containing(PeriodKind.Year, new DateOnly(2026, 6, 1)))).Loaded();

        Assert.Multiple(() =>
        {
            Assert.That(result.Records, Has.Count.EqualTo(4));
            Assert.That(result.PerMonth, Is.EqualTo(new[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0 }));
        });
    }

    [Test]
    public async Task GetRecordsAsync_LogsTheCountAtInformation()
    {
        Setup(Workout(LongAgo, Sets(Squat, Set(5, 60m))), Workout(OctoberStart.AddDays(1), Sets(Squat, Set(5, 70m))));

        await service.GetRecordsAsync(October);

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.PeriodRecordsLoaded, LogLevel.Information, "Loaded 1 records for Month from 2026-10-01") }));
    }

    [Test]
    public async Task GetRecordsAsync_UnknownExercise_IsLeftOutAndLoggedWithItsId()
    {
        Setup(
            Workout(LongAgo, Sets(Squat, Set(5, 60m)), Sets(UnknownId, Set(5, 60m))),
            Workout(OctoberStart.AddDays(1), Sets(Squat, Set(5, 70m)), Sets(UnknownId, Set(5, 70m))));

        var result = await service.GetRecordsAsync(October);

        Assert.That(result.Loaded().Records.Select(r => r.ExerciseId), Is.EqualTo(new[] { Squat }));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.PeriodRecordsUnknownExercise, LogLevel.Warning, $"Exercise {UnknownId} is not in the exercise list; left out"),
            (CoreEvent.PeriodRecordsLoaded, LogLevel.Information, "Loaded 1 records for Month from 2026-10-01"),
        }));
    }

    [Test]
    public async Task GetRecordsAsync_ReadFails_ReturnsFailedAndLogsAWarningWithTheException()
    {
        var error = new InvalidOperationException("disk");
        repository.Setup(r => r.GetAllAsync()).ThrowsAsync(error);

        var result = await service.GetRecordsAsync(October);

        Assert.That(result.Value, Is.InstanceOf<ReadFailed>());
        Assert.Multiple(() =>
        {
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.PeriodRecordsLoadFailed, LogLevel.Warning, "Loading the records for Month from 2026-10-01 failed") }));
            Assert.That(logger.LatestRecord.Exception, Is.SameAs(error));
        });
    }

    [Test]
    public async Task GetRecordsAsync_FindingOneExercisesRecordsFails_LeavesItOutAndLogsAWarningWithTheException()
    {
        var error = new InvalidOperationException("bad set");
        var recordCheck = new Mock<IRecordCheck>();
        recordCheck.Setup(c => c.Records(It.IsAny<ExerciseDefinition>(), It.IsAny<IEnumerable<WorkoutSession>>())).Throws(error);
        service = new PeriodTrainingService(repository.Object, Catalog, recordCheck.Object, new MuscleGroupMap(), new FakeTimeProvider(Now), logger);
        Setup(Workout(OctoberStart, Sets(Squat, Set(5, 100m))));

        var result = await service.GetRecordsAsync(October);

        Assert.That(result.Loaded().Records, Is.Empty);
        Assert.Multiple(() =>
        {
            Assert.That(LoggedEvents.Of(logger).Select(e => e.Id), Does.Contain(CoreEvent.PeriodRecordsExerciseFailed));
            Assert.That(logger.Collector.GetSnapshot().Select(r => r.Exception), Does.Contain(error));
        });
    }

    [Test]
    public async Task GetMuscleSetsAsync_AddsUpEachGroupsSetsInOrder()
    {
        SetUpCounts(OctoberStart, OctoberEnd.AddTicks(-1),
            new ExerciseSetCount(1, Squat, 3), new ExerciseSetCount(1, Curl, 2), new ExerciseSetCount(2, Squat, 4), new ExerciseSetCount(2, Plank, 1));

        var result = await service.GetMuscleSetsAsync(October);

        Assert.That(result.Loaded(), Is.EqualTo([
            new MuscleGroupCount(MuscleGroup.Chest, 0),
            new MuscleGroupCount(MuscleGroup.Back, 0),
            new MuscleGroupCount(MuscleGroup.Shoulders, 0),
            new MuscleGroupCount(MuscleGroup.Arms, 2),
            new MuscleGroupCount(MuscleGroup.Legs, 7),
            new MuscleGroupCount(MuscleGroup.Core, 1),
        ]).UsingPropertiesComparer());
    }

    [Test]
    public async Task GetMuscleSetsAsync_NothingInThePeriod_EveryGroupIsZero()
    {
        SetUpCounts(OctoberStart, OctoberEnd.AddTicks(-1));

        var result = (await service.GetMuscleSetsAsync(October)).Loaded();

        Assert.Multiple(() =>
        {
            Assert.That(result.Select(g => g.Group), Is.EqualTo(Enum.GetValues<MuscleGroup>()));
            Assert.That(result.Select(g => g.Sets), Is.All.Zero);
        });
    }

    [Test]
    public async Task GetMuscleSetsAsync_CardioAndUnknownExercises_AreLeftOut()
    {
        SetUpCounts(OctoberStart, OctoberEnd.AddTicks(-1), new ExerciseSetCount(1, Run, 2), new ExerciseSetCount(1, UnknownId, 5), new ExerciseSetCount(2, UnknownId, 1), new ExerciseSetCount(1, PushUp, 1));

        var result = await service.GetMuscleSetsAsync(October);

        Assert.Multiple(() =>
        {
            Assert.That(result.Loaded().Select(g => g.Sets), Is.EqualTo(new[] { 1, 0, 0, 0, 0, 0 }));
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
            {
                (CoreEvent.PeriodMuscleSetsUnknownExercise, LogLevel.Warning, $"Exercise {UnknownId} is not in the exercise list; left out"),
                (CoreEvent.PeriodMuscleSetsLoaded, LogLevel.Information, "Loaded the sets per muscle group from 1 workouts for Month from 2026-10-01"),
            }));
        });
    }

    [Test]
    public async Task GetMuscleSetsAsync_PastPeriod_ReadsFromTheFirstToTheLastTickOfItsLocalDays()
    {
        SetUpCounts(OctoberStart, OctoberEnd.AddTicks(-1));

        await service.GetMuscleSetsAsync(October);

        repository.VerifyAll();
    }

    [Test]
    public async Task GetMuscleSetsAsync_PeriodHoldingNow_ReadsUpToNow()
    {
        SetUpCounts(new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.FromHours(10)), Now);

        await service.GetMuscleSetsAsync(Period.Containing(PeriodKind.Month, new DateOnly(2026, 11, 3)));

        repository.VerifyAll();
    }

    [Test]
    public async Task GetMuscleSetsAsync_WeekInADstZone_UsesTheOffsetsOfItsFirstAndLastDay()
    {
        UseClock(Now, ConsistencyZones.ForwardOn25Oct);
        var week = Period.Containing(PeriodKind.Week, new DateOnly(2026, 10, 21));
        SetUpCounts(new DateTimeOffset(2026, 10, 19, 0, 0, 0, TimeSpan.FromHours(1)), new DateTimeOffset(2026, 10, 26, 0, 0, 0, TimeSpan.FromHours(2)).AddTicks(-1));

        await service.GetMuscleSetsAsync(week);

        repository.VerifyAll();
    }

    [Test]
    public async Task GetMuscleSetsAsync_MidnightSkipped_PeriodStartsAtTheFirstRealInstant()
    {
        UseClock(Now, ConsistencyZones.MidnightSkippedOn5Oct);
        var week = Period.Containing(PeriodKind.Week, new DateOnly(2026, 10, 5));
        SetUpCounts(new DateTimeOffset(2026, 10, 5, 0, 30, 0, TimeSpan.FromHours(-2)), new DateTimeOffset(2026, 10, 12, 0, 0, 0, TimeSpan.FromHours(-2)).AddTicks(-1));

        await service.GetMuscleSetsAsync(week);

        repository.VerifyAll();
    }

    [Test]
    public async Task GetMuscleSetsAsync_ReadThrows_ReturnsFailedAndLogsAWarningWithTheException()
    {
        var error = new InvalidOperationException("disk");
        repository.Setup(r => r.GetSetCountsAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>())).ThrowsAsync(error);

        var result = await service.GetMuscleSetsAsync(October);

        Assert.That(result.Value, Is.InstanceOf<ReadFailed>());
        Assert.Multiple(() =>
        {
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.PeriodMuscleSetsLoadFailed, LogLevel.Warning, "Loading the sets per muscle group for Month from 2026-10-01 failed") }));
            Assert.That(logger.LatestRecord.Exception, Is.SameAs(error));
        });
    }

    [Test]
    public async Task GetMostTrainedAsync_NothingInThePeriod_IsEmpty()
    {
        Setup(Workout(LongAgo, Sets(Squat, Set(5, 60m))));

        Assert.That((await service.GetMostTrainedAsync(October)).Loaded(), Is.Empty);
    }

    [Test]
    public async Task GetMostTrainedAsync_FewerThanFive_ReturnsThemMostSessionsFirst()
    {
        Setup(
            Workout(OctoberStart, Sets(Squat, Set(5, 60m)), Entry(Run, TimeSpan.FromMinutes(30), 5m)),
            Workout(OctoberStart.AddDays(2), Entry(Run, TimeSpan.FromMinutes(30), 5m), Sets(Curl, Set(10, 10m))),
            Workout(OctoberStart.AddDays(4), Entry(Run, TimeSpan.FromMinutes(30), 5m)),
            Workout(OctoberStart.AddDays(6), Sets(Curl, Set(10, 10m))));

        var result = (await service.GetMostTrainedAsync(October)).Loaded();

        Assert.That(result, Is.EqualTo(new[] { new TrainedExercise(Run, 3), new TrainedExercise(Curl, 2), new TrainedExercise(Squat, 1) }));
    }

    [Test]
    public async Task GetMostTrainedAsync_Ties_AreByNameAToZ()
    {
        Setup(Workout(OctoberStart, Sets(PushUp, Set(10, 0m)), Sets(Squat, Set(5, 60m)), Sets(Curl, Set(10, 10m))));

        var result = (await service.GetMostTrainedAsync(October)).Loaded();

        Assert.That(result, Is.EqualTo(new[] { new TrainedExercise(Squat, 1), new TrainedExercise(Curl, 1), new TrainedExercise(PushUp, 1) }));
    }

    [Test]
    public async Task GetMostTrainedAsync_MoreThanFive_TakesTheTopFive()
    {
        Setup(
            Workout(OctoberStart, Sets(Squat, Set(5, 60m)), Sets(Curl, Set(10, 10m)), Sets(PushUp, Set(10, 0m)), Sets(Plank, Set(null, null, TimeSpan.FromMinutes(1))),
                Sets(AssistedPullUp, Set(10, 30m)), Sets(FarmersWalk, Set(null, 20m, TimeSpan.FromMinutes(1), 20m))),
            Workout(OctoberStart.AddDays(1), Sets(FarmersWalk, Set(null, 20m, TimeSpan.FromMinutes(1), 20m))));

        var result = (await service.GetMostTrainedAsync(October)).Loaded();

        Assert.That(result, Is.EqualTo(new[]
        {
            new TrainedExercise(FarmersWalk, 2),
            new TrainedExercise(Squat, 1),
            new TrainedExercise(Curl, 1),
            new TrainedExercise(AssistedPullUp, 1),
            new TrainedExercise(Plank, 1),
        }));
    }

    [Test]
    public async Task GetMostTrainedAsync_FirstAndLastMomentOfThePeriod_AreInAndJustOutsideAreOut()
    {
        Setup(
            Workout(OctoberStart.AddTicks(-1), Sets(Squat, Set(5, 60m))),
            Workout(OctoberStart, Sets(Curl, Set(10, 10m))),
            Workout(OctoberEnd.AddTicks(-1), Sets(Curl, Set(10, 10m))),
            Workout(OctoberEnd, Sets(Squat, Set(5, 60m))));

        Assert.That((await service.GetMostTrainedAsync(October)).Loaded(), Is.EqualTo(new[] { new TrainedExercise(Curl, 2) }));
    }

    [Test]
    public async Task GetMostTrainedAsync_AfterNow_IsOut()
    {
        Setup(Workout(Now.AddMinutes(1), Sets(Squat, Set(5, 60m))), Workout(Now, Sets(Curl, Set(10, 10m))));

        var result = await service.GetMostTrainedAsync(Period.Containing(PeriodKind.Month, new DateOnly(2026, 11, 15)));

        Assert.That(result.Loaded(), Is.EqualTo(new[] { new TrainedExercise(Curl, 1) }));
    }

    [Test]
    public async Task GetMostTrainedAsync_UnknownExercise_IsLeftOutAndLoggedOncePerLoad()
    {
        Setup(Workout(OctoberStart, Sets(UnknownId, Set(5, 60m)), Sets(Squat, Set(5, 60m))), Workout(OctoberStart.AddDays(1), Sets(UnknownId, Set(5, 60m))));

        var result = await service.GetMostTrainedAsync(October);

        Assert.That(result.Loaded(), Is.EqualTo(new[] { new TrainedExercise(Squat, 1) }));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.PeriodMostTrainedUnknownExercise, LogLevel.Warning, $"Exercise {UnknownId} is not in the exercise list; left out"),
            (CoreEvent.PeriodMostTrainedLoaded, LogLevel.Information, "Loaded 1 most trained exercises for Month from 2026-10-01"),
        }));
    }

    [Test]
    public async Task GetMostTrainedAsync_ReadFails_ReturnsFailedAndLogsAWarningWithTheException()
    {
        var error = new InvalidOperationException("disk");
        repository.Setup(r => r.GetAllAsync()).ThrowsAsync(error);

        var result = await service.GetMostTrainedAsync(October);

        Assert.That(result.Value, Is.InstanceOf<ReadFailed>());
        Assert.Multiple(() =>
        {
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.PeriodMostTrainedLoadFailed, LogLevel.Warning, "Loading the most trained exercises for Month from 2026-10-01 failed") }));
            Assert.That(logger.LatestRecord.Exception, Is.SameAs(error));
        });
    }

    [Test]
    public async Task GetMostTrainedAsync_ReadsTheExerciseListOnce()
    {
        var catalog = new Mock<IExerciseCatalog>();
        catalog.Setup(c => c.All).Returns(Catalog.All);
        service = new PeriodTrainingService(repository.Object, catalog.Object, Mock.Of<IRecordCheck>(), new MuscleGroupMap(), new FakeTimeProvider(Now), logger);
        Setup();

        await service.GetMostTrainedAsync(October);
        await service.GetMostTrainedAsync(October);

        catalog.Verify(c => c.All, Times.Once);
    }

    private void UseClock(DateTimeOffset now, TimeZoneInfo zone)
    {
        var clock = new FakeTimeProvider(now);
        clock.SetLocalTimeZone(zone);
        IBestRule[] rules =
        [
            new LiftBestRule(new TopSetRule()), new BodyweightBestRule(new BodyweightRule(new TopSetRule(), new E1rmRule())), new AssistedBestRule(new LeastAssistRule()),
            new HoldBestRule(new HoldRule()), new CarryBestRule(new CarryRule()), new CardioBestRule(new CardioSessionRule()),
        ];
        service = new PeriodTrainingService(repository.Object, Catalog, new RecordCheck(rules), new MuscleGroupMap(), clock, logger);
    }

    private Workout Workout(DateTimeOffset startedAt, params WorkoutExercise[] exercises) =>
        new(++nextId, startedAt, startedAt.AddHours(1), exercises);

    private void Setup(params Workout[] workouts) =>
        repository.Setup(r => r.GetAllAsync()).ReturnsAsync(workouts);

    private void SetUpCounts(DateTimeOffset from, DateTimeOffset to, params ExerciseSetCount[] counts) =>
        repository.Setup(r => r.GetSetCountsAsync(from, to)).ReturnsAsync(counts);
}
