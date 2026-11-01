using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
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
    public void Change_NoPrevious_IsFirstTime() =>
        Assert.That(rule.Change(Definition(Squat), new ExerciseBest(100m, 5, null, null), null), Is.EqualTo(new BestChange(BestChangeKind.FirstTime, null)));

    [Test]
    public void Change_Tie_IsEqual() =>
        Assert.That(rule.Change(Definition(Squat), new ExerciseBest(100m, 5, null, null), new ExerciseBest(100m, 5, null, null)), Is.EqualTo(new BestChange(BestChangeKind.Equal, null)));

    [Test]
    public void Change_HeavierLift_IsUpByTheWeight() =>
        Assert.That(rule.Change(Definition(Squat), new ExerciseBest(102.5m, 3, null, null), new ExerciseBest(100m, 5, null, null)),
            Is.EqualTo(new BestChange(BestChangeKind.Up, new ExerciseBest(2.5m, null, null, null))));

    [Test]
    public void Change_LighterLift_IsDownByTheWeight() =>
        Assert.That(rule.Change(Definition(Squat), new ExerciseBest(95m, 8, null, null), new ExerciseBest(100m, 5, null, null)),
            Is.EqualTo(new BestChange(BestChangeKind.Down, new ExerciseBest(5m, null, null, null))));

    [Test]
    public void Change_SameWeightMoreReps_IsUpByTheReps() =>
        Assert.That(rule.Change(Definition(Squat), new ExerciseBest(100m, 7, null, null), new ExerciseBest(100m, 5, null, null)),
            Is.EqualTo(new BestChange(BestChangeKind.Up, new ExerciseBest(null, 2, null, null))));

    [Test]
    public void Change_BodyweightMoreReps_IsUpByTheReps() =>
        Assert.That(rule.Change(Definition(PushUp), new ExerciseBest(null, 25, null, null), new ExerciseBest(null, 20, null, null)),
            Is.EqualTo(new BestChange(BestChangeKind.Up, new ExerciseBest(null, 5, null, null))));

    [Test]
    public void Change_AssistedLessAssist_IsUp() =>
        Assert.That(rule.Change(Definition(AssistedPullUp), new ExerciseBest(20m, 8, null, null), new ExerciseBest(25m, 10, null, null)),
            Is.EqualTo(new BestChange(BestChangeKind.Up, new ExerciseBest(5m, null, null, null))));

    [Test]
    public void Change_AssistedMoreAssist_IsDown() =>
        Assert.That(rule.Change(Definition(AssistedPullUp), new ExerciseBest(30m, 8, null, null), new ExerciseBest(25m, 10, null, null)),
            Is.EqualTo(new BestChange(BestChangeKind.Down, new ExerciseBest(5m, null, null, null))));

    [Test]
    public void Change_LongerHold_IsUpByTheDuration() =>
        Assert.That(rule.Change(Definition(Plank), new ExerciseBest(null, null, TimeSpan.FromSeconds(75), null), new ExerciseBest(null, null, TimeSpan.FromSeconds(60), null)),
            Is.EqualTo(new BestChange(BestChangeKind.Up, new ExerciseBest(null, null, TimeSpan.FromSeconds(15), null))));

    [Test]
    public void Change_HeavierCarry_IsUpByTheWeight() =>
        Assert.That(rule.Change(Definition(FarmersWalk), new ExerciseBest(40m, null, null, 30m), new ExerciseBest(32m, null, null, 40m)),
            Is.EqualTo(new BestChange(BestChangeKind.Up, new ExerciseBest(8m, null, null, null))));

    [Test]
    public void Change_CarrySameWeightShorterDistance_IsDownByTheDistance() =>
        Assert.That(rule.Change(Definition(FarmersWalk), new ExerciseBest(32m, null, null, 30m), new ExerciseBest(32m, null, null, 40m)),
            Is.EqualTo(new BestChange(BestChangeKind.Down, new ExerciseBest(null, null, null, 10m))));

    [Test]
    public void Change_FartherCardio_IsUpByTheDistance() =>
        Assert.That(rule.Change(Definition(Run), new ExerciseBest(null, null, null, 6m), new ExerciseBest(null, null, null, 5.2m)),
            Is.EqualTo(new BestChange(BestChangeKind.Up, new ExerciseBest(null, null, null, 0.8m))));

    [Test]
    public void Change_ShorterCardioDuration_IsDownByTheDuration() =>
        Assert.That(rule.Change(Definition(StairClimber), new ExerciseBest(null, null, TimeSpan.FromMinutes(10), null), new ExerciseBest(null, null, TimeSpan.FromMinutes(15), null)),
            Is.EqualTo(new BestChange(BestChangeKind.Down, new ExerciseBest(null, null, TimeSpan.FromMinutes(5), null))));
}
