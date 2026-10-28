using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using static WorkoutLog.Core.Tests.Domain.Exercises.Bests.BestRuleCases;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Bests;

public class CarryBestRuleTests
{
    private readonly CarryBestRule rule = new(new CarryRule());

    [Test]
    public void Exercise_IsTheOneNamed() =>
        Assert.That(Definition(FarmersWalk).Name, Is.EqualTo("Farmer's Walk"));

    [TestCase(FarmersWalk, true)]
    [TestCase(Squat, false)]
    [TestCase(PushUp, false)]
    [TestCase(Plank, false)]
    [TestCase(Run, false)]
    public void AppliesTo_CarriesOnly(int id, bool expected) =>
        Assert.That(rule.AppliesTo(Definition(id)), Is.EqualTo(expected));

    [Test]
    public void Best_NoSets_IsNull() =>
        Assert.That(rule.Best(Definition(FarmersWalk), Sets(FarmersWalk)), Is.Null);

    [Test]
    public void Best_OneSet_IsItsWeightAndDistance() =>
        Assert.That(rule.Best(Definition(FarmersWalk), Sets(FarmersWalk, Set(null, 32m, null, 40m))),
            Is.EqualTo(new ExerciseBest(32m, null, null, 40m)));

    [Test]
    public void Best_ManySets_IsTheHeaviest() =>
        Assert.That(rule.Best(Definition(FarmersWalk), Sets(FarmersWalk, Set(null, 32m, null, 40m), Set(null, 36m, null, 20m), Set(null, 24m, null, 60m))),
            Is.EqualTo(new ExerciseBest(36m, null, null, 20m)));

    [Test]
    public void Best_TiedWeight_TakesTheLongest() =>
        Assert.That(rule.Best(Definition(FarmersWalk), Sets(FarmersWalk, Set(null, 32m, null, 40m), Set(null, 32m, null, 50m))),
            Is.EqualTo(new ExerciseBest(32m, null, null, 50m)));

    [Test]
    public void Best_NoSetHasAWeight_IsNull() =>
        Assert.That(rule.Best(Definition(FarmersWalk), Sets(FarmersWalk, Set(null, null, null, 40m))), Is.Null);

    [Test]
    public void Best_ZeroKgSets_AreIgnored() =>
        Assert.That(rule.Best(Definition(FarmersWalk), Sets(FarmersWalk, Set(null, 0m, null, 100m), Set(null, 10m, null, 20m))),
            Is.EqualTo(new ExerciseBest(10m, null, null, 20m)));

    [Test]
    public void Best_OnlyZeroKgSets_IsNull() =>
        Assert.That(rule.Best(Definition(FarmersWalk), Sets(FarmersWalk, Set(null, 0m, null, 40m))), Is.Null);
}
