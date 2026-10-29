using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using static WorkoutLog.Core.Tests.Domain.Exercises.Bests.BestRuleCases;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Bests;

public class LiftBestRuleTests
{
    private readonly LiftBestRule rule = new(new TopSetRule());

    [Test]
    public void Exercises_AreTheOnesNamed() =>
        Assert.That(new[] { Squat, Curl, PlateFrontRaise }.Select(id => Definition(id).Name), Is.EqualTo(new[] { "Barbell Back Squat", "Dumbbell Curl", "Plate Front Raise" }));

    [TestCase(Squat, true)]
    [TestCase(Curl, true)]
    [TestCase(PlateFrontRaise, true)]
    [TestCase(PushUp, false)]
    [TestCase(BandPullApart, false)]
    [TestCase(AssistedPullUp, false)]
    [TestCase(Plank, false)]
    [TestCase(FarmersWalk, false)]
    [TestCase(Run, false)]
    public void AppliesTo_LiftsOnly(int id, bool expected) =>
        Assert.That(rule.AppliesTo(Definition(id)), Is.EqualTo(expected));

    [Test]
    public void Best_NoSets_IsNull() =>
        Assert.That(rule.Best(Definition(Squat), Sets(Squat)), Is.Null);

    [Test]
    public void Best_OneSet_IsThatSet() =>
        Assert.That(rule.Best(Definition(Squat), Sets(Squat, Set(5, 60m))), Is.EqualTo(new ExerciseBest(60m, 5, null, null)));

    [Test]
    public void Best_ManySets_IsTheHeaviest() =>
        Assert.That(rule.Best(Definition(Squat), Sets(Squat, Set(8, 60m), Set(3, 70m), Set(6, 65m))),
            Is.EqualTo(new ExerciseBest(70m, 3, null, null)));

    [Test]
    public void Best_TiedWeight_TakesTheMostReps() =>
        Assert.That(rule.Best(Definition(Squat), Sets(Squat, Set(3, 70m), Set(5, 70m), Set(4, 70m))),
            Is.EqualTo(new ExerciseBest(70m, 5, null, null)));

    [Test]
    public void Best_SetsWithoutWeight_AreIgnored() =>
        Assert.That(rule.Best(Definition(Squat), Sets(Squat, Set(12, null), Set(5, 60m))), Is.EqualTo(new ExerciseBest(60m, 5, null, null)));

    [Test]
    public void Best_NoSetHasAWeight_IsNull() =>
        Assert.That(rule.Best(Definition(Squat), Sets(Squat, Set(12, null), Set(10, null))), Is.Null);
}
