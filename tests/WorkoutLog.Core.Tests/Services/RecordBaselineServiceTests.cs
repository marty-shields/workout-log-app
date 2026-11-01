using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;
using static WorkoutLog.Core.Tests.Domain.Exercises.Bests.BestRuleCases;

namespace WorkoutLog.Core.Tests.Services;

public class RecordBaselineServiceTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 1, 18, 0, 0, TimeSpan.Zero);

    private static readonly IBestRule[] Rules =
    [
        new LiftBestRule(new TopSetRule()), new BodyweightBestRule(new BodyweightRule(new TopSetRule(), new E1rmRule())), new AssistedBestRule(new LeastAssistRule()),
        new HoldBestRule(new HoldRule()), new CarryBestRule(new CarryRule()), new CardioBestRule(new CardioSessionRule()),
    ];

    private Mock<IWorkoutRepository> repository = null!;
    private FakeLogger<RecordBaselineService> logger = null!;
    private RecordBaselineService service = null!;

    [SetUp]
    public void SetUp()
    {
        repository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        logger = new FakeLogger<RecordBaselineService>();
        service = new RecordBaselineService(repository.Object, Catalog, Rules, new RecordCheck(Rules), logger);
    }

    [Test]
    public async Task LoadAsync_Saved_ReturnsABaselineThatIsNotFailed()
    {
        Saved(Squat, Sets(Squat, Set(5, 60m)));

        var result = await service.LoadAsync(Squat);

        Assert.That(result.Failed, Is.False);
        Assert.That(result.Baseline, Is.Not.Null);
    }

    [Test]
    public async Task LoadAsync_Saved_LogsNothing()
    {
        Saved(Squat);

        await service.LoadAsync(Squat);

        Assert.That(LoggedEvents.Of(logger), Is.Empty);
    }

    [Test]
    public async Task LoadAsync_RepositoryThrows_ReturnsFailedWithNoBaseline()
    {
        repository.Setup(r => r.GetSessionsAsync(Squat)).ThrowsAsync(new InvalidOperationException("disk"));

        Assert.That(await service.LoadAsync(Squat), Is.EqualTo(new RecordBaselineResult(null, Failed: true)));
    }

    [Test]
    public async Task LoadAsync_RepositoryThrows_LogsAWarningWithTheExerciseIdAndException()
    {
        var exception = new InvalidOperationException("disk");
        repository.Setup(r => r.GetSessionsAsync(Squat)).ThrowsAsync(exception);

        await service.LoadAsync(Squat);

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.RecordBaselineLoadFailed, LogLevel.Warning, $"Loading the record baseline of exercise {Squat} failed"),
        }));
        Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
    }

    [Test]
    public async Task LoadAsync_UnknownExercise_ReturnsFailed() =>
        Assert.That((await service.LoadAsync(9999)).Failed, Is.True);

    [Test]
    public async Task RecordEntryIndex_Lift_IsTheTopSetWhenItBeatsEverySavedWorkout()
    {
        var baseline = await Load(Squat, Sets(Squat, Set(5, 60m)), Sets(Squat, Set(3, 70m)));

        Assert.That(baseline.RecordEntryIndex(Sets(Squat, Set(5, 60m), Set(1, 72.5m), Set(8, 50m))), Is.EqualTo(1));
    }

    [Test]
    public async Task RecordEntryIndex_Lift_MoreRepsAtTheSameWeightBeatsTheSavedBest()
    {
        var baseline = await Load(Squat, Sets(Squat, Set(5, 70m)));

        Assert.That(baseline.RecordEntryIndex(Sets(Squat, Set(5, 60m), Set(6, 70m))), Is.EqualTo(1));
    }

    [Test]
    public async Task RecordEntryIndex_Lift_ASetBelowTheSavedBestIsNoRecord()
    {
        var baseline = await Load(Squat, Sets(Squat, Set(3, 70m)));

        Assert.That(baseline.RecordEntryIndex(Sets(Squat, Set(5, 60m), Set(1, 65m))), Is.Null);
    }

    [Test]
    public async Task RecordEntryIndex_Tie_IsNoRecord()
    {
        var baseline = await Load(Squat, Sets(Squat, Set(5, 60m)));

        Assert.That(baseline.RecordEntryIndex(Sets(Squat, Set(5, 60m))), Is.Null);
    }

    [Test]
    public async Task RecordEntryIndex_FirstSession_IsNoRecord()
    {
        var baseline = await Load(Squat);

        Assert.That(baseline.RecordEntryIndex(Sets(Squat, Set(5, 200m))), Is.Null);
    }

    [Test]
    public async Task RecordEntryIndex_TwoSetsGiveTheSameBest_IsTheFirst()
    {
        var baseline = await Load(Squat, Sets(Squat, Set(5, 60m)));

        Assert.That(baseline.RecordEntryIndex(Sets(Squat, Set(5, 80m), Set(5, 80m))), Is.EqualTo(0));
    }

    [Test]
    public async Task RecordEntryIndex_NoEntries_IsNull()
    {
        var baseline = await Load(Squat, Sets(Squat, Set(5, 60m)));

        Assert.That(baseline.RecordEntryIndex(Sets(Squat)), Is.Null);
    }

    [Test]
    public async Task RecordEntryIndex_BodyweightRepsOnly_IsTheMostReps()
    {
        var baseline = await Load(PushUp, Sets(PushUp, Set(20, null)));

        Assert.That(baseline.RecordEntryIndex(Sets(PushUp, Set(15, null), Set(22, null), Set(18, null))), Is.EqualTo(1));
    }

    [Test]
    public async Task RecordEntryIndex_BodyweightWeighted_IsTheHeaviestSet()
    {
        var baseline = await Load(PushUp, Sets(PushUp, Set(10, 5m)));

        Assert.That(baseline.RecordEntryIndex(Sets(PushUp, Set(30, null), Set(8, 10m))), Is.EqualTo(1));
    }

    [Test]
    public async Task RecordEntryIndex_Assisted_LessAssistanceIsBetter()
    {
        var baseline = await Load(AssistedPullUp, Sets(AssistedPullUp, Set(8, 25m)));

        Assert.That(baseline.RecordEntryIndex(Sets(AssistedPullUp, Set(8, 30m), Set(6, 20m), Set(8, 40m))), Is.EqualTo(1));
    }

    [Test]
    public async Task RecordEntryIndex_Assisted_MoreAssistanceIsNoRecord()
    {
        var baseline = await Load(AssistedPullUp, Sets(AssistedPullUp, Set(8, 25m)));

        Assert.That(baseline.RecordEntryIndex(Sets(AssistedPullUp, Set(8, 30m))), Is.Null);
    }

    [Test]
    public async Task RecordEntryIndex_Hold_IsTheLongestDuration()
    {
        var baseline = await Load(Plank, Sets(Plank, Set(null, 0m, TimeSpan.FromSeconds(60))));

        Assert.That(baseline.RecordEntryIndex(Sets(Plank, Set(null, 0m, TimeSpan.FromSeconds(45)), Set(null, 0m, TimeSpan.FromSeconds(75)))), Is.EqualTo(1));
    }

    [Test]
    public async Task RecordEntryIndex_Carry_IsTheHeaviestThenFurthest()
    {
        var baseline = await Load(FarmersWalk, Sets(FarmersWalk, Set(null, 40m, null, 30m)));

        Assert.That(baseline.RecordEntryIndex(Sets(FarmersWalk, Set(null, 40m, null, 50m), Set(null, 30m, null, 100m))), Is.EqualTo(0));
    }

    [Test]
    public async Task RecordEntryIndex_CardioWithDistance_IsTheDistance()
    {
        var baseline = await Load(Run, Entry(Run, TimeSpan.FromMinutes(30), 5m));

        Assert.That(baseline.RecordEntryIndex(Entry(Run, TimeSpan.FromMinutes(60), 5.5m)), Is.EqualTo(0));
        Assert.That(baseline.RecordEntryIndex(Entry(Run, TimeSpan.FromMinutes(20), 4m)), Is.Null);
    }

    [Test]
    public async Task RecordEntryIndex_CardioWithoutDistance_IsTheDuration()
    {
        var baseline = await Load(JumpRope, Entry(JumpRope, TimeSpan.FromMinutes(10), null));

        Assert.That(baseline.RecordEntryIndex(Entry(JumpRope, TimeSpan.FromMinutes(12), null)), Is.EqualTo(0));
        Assert.That(baseline.RecordEntryIndex(Entry(JumpRope, TimeSpan.FromMinutes(10), null)), Is.Null);
    }

    [Test]
    public async Task RecordEntryIndex_SavedWorkoutsOutOfOrder_ComparesWithTheBestOfAll()
    {
        var baseline = await Load(Squat, Sets(Squat, Set(5, 100m)), Sets(Squat, Set(5, 60m)));

        Assert.That(baseline.RecordEntryIndex(Sets(Squat, Set(5, 80m))), Is.Null);
    }

    private async Task<RecordBaseline> Load(int exerciseId, params WorkoutExercise[] saved)
    {
        Saved(exerciseId, saved);
        return (await service.LoadAsync(exerciseId)).Baseline!;
    }

    private void Saved(int exerciseId, params WorkoutExercise[] saved) =>
        repository.Setup(r => r.GetSessionsAsync(exerciseId))
            .ReturnsAsync(saved.Select((exercise, i) => new WorkoutSession(i + 1, Start.AddDays(i), exercise)).ToList());
}
