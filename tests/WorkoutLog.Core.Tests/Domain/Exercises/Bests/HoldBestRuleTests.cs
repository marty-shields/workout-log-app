using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using static WorkoutLog.Core.Tests.Domain.Exercises.Bests.BestRuleCases;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Bests;

public class HoldBestRuleTests
{
    private readonly HoldBestRule rule = new(new HoldRule());

    [Test]
    public void Exercise_IsTheOneNamed() =>
        Assert.That(Definition(Plank).Name, Is.EqualTo("Plank"));

    [TestCase(Plank, true)]
    [TestCase(Squat, false)]
    [TestCase(PushUp, false)]
    [TestCase(FarmersWalk, false)]
    [TestCase(JumpRope, false)]
    public void AppliesTo_HoldsOnly(int id, bool expected) =>
        Assert.That(rule.AppliesTo(Definition(id)), Is.EqualTo(expected));

    [Test]
    public void Best_NoSets_IsNull() =>
        Assert.That(rule.Best(Definition(Plank), Sets(Plank)), Is.Null);

    [Test]
    public void Best_OneSet_IsItsDuration() =>
        Assert.That(rule.Best(Definition(Plank), Sets(Plank, Set(null, null, TimeSpan.FromSeconds(60)))),
            Is.EqualTo(new ExerciseBest(null, null, TimeSpan.FromSeconds(60), null)));

    [Test]
    public void Best_ManySets_IsTheLongest() =>
        Assert.That(rule.Best(Definition(Plank), Sets(Plank,
                Set(null, 10m, TimeSpan.FromSeconds(60)), Set(null, null, TimeSpan.FromSeconds(105)), Set(null, null, TimeSpan.FromSeconds(90)))),
            Is.EqualTo(new ExerciseBest(null, null, TimeSpan.FromSeconds(105), null)));

    [Test]
    public void Best_NoSetHasADuration_IsNull() =>
        Assert.That(rule.Best(Definition(Plank), Sets(Plank, Set(null, 10m))), Is.Null);
}
