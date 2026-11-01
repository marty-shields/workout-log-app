using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using static WorkoutLog.Core.Tests.Domain.Exercises.Bests.BestRuleCases;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Bests;

public class BestChangeRuleTests
{
    private readonly BestChangeRule rule = new(
    [
        new LiftBestRule(new TopSetRule()), new BodyweightBestRule(new BodyweightRule(new TopSetRule(), new E1rmRule())), new AssistedBestRule(new LeastAssistRule()),
        new HoldBestRule(new HoldRule()), new CarryBestRule(new CarryRule()), new CardioBestRule(new CardioSessionRule()),
    ]);

    [Test]
    public void Change_NoBestInTheSession_IsNull() =>
        Assert.That(rule.Change(Definition(Squat), Sets(Squat), [Sets(Squat, Set(5, 100m))]), Is.Null);

    [Test]
    public void Change_NoEarlierSession_IsFirstTimeWithTheBest() =>
        Assert.That(rule.Change(Definition(Squat), Sets(Squat, Set(5, 100m)), []),
            Is.EqualTo(new SessionChange(new ExerciseBest(100m, 5, null, null), new BestChange(BestChangeKind.FirstTime, null))));

    [Test]
    public void Change_EarlierSessionsWithoutABest_IsFirstTime() =>
        Assert.That(rule.Change(Definition(Squat), Sets(Squat, Set(5, 100m)), [Sets(Squat)])!.Change, Is.EqualTo(new BestChange(BestChangeKind.FirstTime, null)));

    [Test]
    public void Change_ComparesWithTheLastEarlierSessionNotTheBestOne() =>
        Assert.That(rule.Change(Definition(Squat), Sets(Squat, Set(5, 95m)), [Sets(Squat, Set(5, 120m)), Sets(Squat, Set(5, 90m)), Sets(Squat)])!.Change,
            Is.EqualTo(new BestChange(BestChangeKind.Up, new ExerciseBest(5m, null, null, null))));

    [Test]
    public void Change_Tie_IsEqual() =>
        Assert.That(rule.Change(Definition(Squat), Sets(Squat, Set(5, 100m)), [Sets(Squat, Set(5, 100m))])!.Change, Is.EqualTo(new BestChange(BestChangeKind.Equal, null)));

    [Test]
    public void Change_HeavierLift_IsUpByTheWeight() =>
        Assert.That(rule.Change(Definition(Squat), Sets(Squat, Set(3, 102.5m)), [Sets(Squat, Set(5, 100m))])!.Change,
            Is.EqualTo(new BestChange(BestChangeKind.Up, new ExerciseBest(2.5m, null, null, null))));

    [Test]
    public void Change_LighterLift_IsDownByTheWeight() =>
        Assert.That(rule.Change(Definition(Squat), Sets(Squat, Set(8, 95m)), [Sets(Squat, Set(5, 100m))])!.Change,
            Is.EqualTo(new BestChange(BestChangeKind.Down, new ExerciseBest(5m, null, null, null))));

    [Test]
    public void Change_SameWeightMoreReps_IsUpByTheReps() =>
        Assert.That(rule.Change(Definition(Squat), Sets(Squat, Set(7, 100m)), [Sets(Squat, Set(5, 100m))])!.Change,
            Is.EqualTo(new BestChange(BestChangeKind.Up, new ExerciseBest(null, 2, null, null))));

    [Test]
    public void Change_BodyweightMoreReps_IsUpByTheReps() =>
        Assert.That(rule.Change(Definition(PushUp), Sets(PushUp, Set(25, 0m)), [Sets(PushUp, Set(20, 0m))])!.Change,
            Is.EqualTo(new BestChange(BestChangeKind.Up, new ExerciseBest(null, 5, null, null))));

    [Test]
    public void Change_AssistedLessAssist_IsUp() =>
        Assert.That(rule.Change(Definition(AssistedPullUp), Sets(AssistedPullUp, Set(8, 20m)), [Sets(AssistedPullUp, Set(10, 25m))])!.Change,
            Is.EqualTo(new BestChange(BestChangeKind.Up, new ExerciseBest(5m, null, null, null))));

    [Test]
    public void Change_AssistedMoreAssist_IsDown() =>
        Assert.That(rule.Change(Definition(AssistedPullUp), Sets(AssistedPullUp, Set(8, 30m)), [Sets(AssistedPullUp, Set(10, 25m))])!.Change,
            Is.EqualTo(new BestChange(BestChangeKind.Down, new ExerciseBest(5m, null, null, null))));

    [Test]
    public void Change_LongerHold_IsUpByTheDuration() =>
        Assert.That(rule.Change(Definition(Plank), Sets(Plank, Set(null, null, TimeSpan.FromSeconds(75))), [Sets(Plank, Set(null, null, TimeSpan.FromSeconds(60)))])!.Change,
            Is.EqualTo(new BestChange(BestChangeKind.Up, new ExerciseBest(null, null, TimeSpan.FromSeconds(15), null))));

    [Test]
    public void Change_HeavierCarry_IsUpByTheWeight() =>
        Assert.That(rule.Change(Definition(FarmersWalk), Sets(FarmersWalk, Set(null, 40m, TimeSpan.FromSeconds(30), 30m)), [Sets(FarmersWalk, Set(null, 32m, TimeSpan.FromSeconds(30), 40m))])!.Change,
            Is.EqualTo(new BestChange(BestChangeKind.Up, new ExerciseBest(8m, null, null, null))));

    [Test]
    public void Change_CarrySameWeightShorterDistance_IsDownByTheDistance() =>
        Assert.That(rule.Change(Definition(FarmersWalk), Sets(FarmersWalk, Set(null, 32m, TimeSpan.FromSeconds(30), 30m)), [Sets(FarmersWalk, Set(null, 32m, TimeSpan.FromSeconds(30), 40m))])!.Change,
            Is.EqualTo(new BestChange(BestChangeKind.Down, new ExerciseBest(null, null, null, 10m))));

    [Test]
    public void Change_FartherCardio_IsUpByTheDistance() =>
        Assert.That(rule.Change(Definition(Run), Entry(Run, TimeSpan.FromMinutes(30), 6m), [Entry(Run, TimeSpan.FromMinutes(30), 5.2m)])!.Change,
            Is.EqualTo(new BestChange(BestChangeKind.Up, new ExerciseBest(null, null, null, 0.8m))));

    [Test]
    public void Change_ShorterCardioDuration_IsDownByTheDuration() =>
        Assert.That(rule.Change(Definition(StairClimber), Entry(StairClimber, TimeSpan.FromMinutes(10), null), [Entry(StairClimber, TimeSpan.FromMinutes(15), null)])!.Change,
            Is.EqualTo(new BestChange(BestChangeKind.Down, new ExerciseBest(null, null, TimeSpan.FromMinutes(5), null))));

    [Test]
    public void Change_BestsThatDifferInNoShownField_Throws() =>
        Assert.Throws<InvalidOperationException>(() => new BestChangeRule([new OddRule()]).Change(Definition(Squat), Sets(Squat, Set(5, 100m)), [Sets(Squat, Set(5, 100m))]));

    /// <summary>Calls every pair different without any field of the best differing.</summary>
    private sealed class OddRule : IBestRule
    {
        public bool AppliesTo(ExerciseDefinition exercise) => true;

        public ExerciseBest? Best(ExerciseDefinition exercise, WorkoutExercise logged) => new(100m, 5, null, null);

        public int Compare(ExerciseBest x, ExerciseBest y) => 1;
    }
}
