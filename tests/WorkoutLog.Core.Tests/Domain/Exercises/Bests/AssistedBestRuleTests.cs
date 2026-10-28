using WorkoutLog.Core.Domain.Exercises.Bests;
using static WorkoutLog.Core.Tests.Domain.Exercises.Bests.BestRuleCases;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Bests;

public class AssistedBestRuleTests
{
    private readonly AssistedBestRule rule = new();

    [Test]
    public void Exercise_IsTheOneNamed() =>
        Assert.That(Definition(AssistedPullUp).Name, Is.EqualTo("Machine Assisted Pull-up"));

    [TestCase(AssistedPullUp, true)]
    [TestCase(Squat, false)]
    [TestCase(PushUp, false)]
    [TestCase(Plank, false)]
    [TestCase(Run, false)]
    public void AppliesTo_AssistedOnly(int id, bool expected) =>
        Assert.That(rule.AppliesTo(Definition(id)), Is.EqualTo(expected));

    [Test]
    public void Best_NoSets_IsNull() =>
        Assert.That(rule.Best(Definition(AssistedPullUp), Sets(AssistedPullUp)), Is.Null);

    [Test]
    public void Best_OneSet_IsThatSet() =>
        Assert.That(rule.Best(Definition(AssistedPullUp), Sets(AssistedPullUp, Set(10, 25m))), Is.EqualTo(new ExerciseBest(25m, 10, null, null)));

    [Test]
    public void Best_ManySets_IsTheLeastAssist() =>
        Assert.That(rule.Best(Definition(AssistedPullUp), Sets(AssistedPullUp, Set(10, 25m), Set(8, 20m), Set(10, 30m))),
            Is.EqualTo(new ExerciseBest(20m, 8, null, null)));

    [Test]
    public void Best_TiedAssist_TakesTheMostReps() =>
        Assert.That(rule.Best(Definition(AssistedPullUp), Sets(AssistedPullUp, Set(8, 20m), Set(10, 20m))),
            Is.EqualTo(new ExerciseBest(20m, 10, null, null)));

    [Test]
    public void Best_BlankAssist_Is0Kg() =>
        Assert.That(rule.Best(Definition(AssistedPullUp), Sets(AssistedPullUp, Set(10, 20m), Set(4, null))),
            Is.EqualTo(new ExerciseBest(0m, 4, null, null)));
}
