using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using static WorkoutLog.Core.Tests.Domain.Exercises.Bests.BestRuleCases;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Bests;

public class BodyweightBestRuleTests
{
    private readonly BodyweightBestRule rule = new(new BodyweightRule(new TopSetRule(), new E1rmRule()));

    [Test]
    public void Exercises_AreTheOnesNamed() =>
        Assert.That(new[] { PushUp, BandPullApart }.Select(id => Definition(id).Name), Is.EqualTo(new[] { "Push-up", "Band Pull-Apart" }));

    [TestCase(PushUp, true)]
    [TestCase(BandPullApart, true)]
    [TestCase(PlateFrontRaise, false)]
    [TestCase(Squat, false)]
    [TestCase(AssistedPullUp, false)]
    [TestCase(Plank, false)]
    [TestCase(FarmersWalk, false)]
    [TestCase(JumpRope, false)]
    public void AppliesTo_BodyweightStyleOnly(int id, bool expected) =>
        Assert.That(rule.AppliesTo(Definition(id)), Is.EqualTo(expected));

    [Test]
    public void Best_NoSets_IsNull() =>
        Assert.That(rule.Best(Definition(PushUp), Sets(PushUp)), Is.Null);

    [Test]
    public void Best_OneUnweightedSet_IsItsReps() =>
        Assert.That(rule.Best(Definition(PushUp), Sets(PushUp, Set(25, null))), Is.EqualTo(new ExerciseBest(null, 25, null, null)));

    [Test]
    public void Best_ManyUnweightedSets_IsTheMostReps() =>
        Assert.That(rule.Best(Definition(PushUp), Sets(PushUp, Set(20, null), Set(25, null), Set(18, null))),
            Is.EqualTo(new ExerciseBest(null, 25, null, null)));

    [Test]
    public void Best_AWeightedSet_IsTheTopSet() =>
        Assert.That(rule.Best(Definition(PushUp), Sets(PushUp, Set(30, null), Set(8, 10m), Set(10, 5m))),
            Is.EqualTo(new ExerciseBest(10m, 8, null, null)));

    [Test]
    public void Best_ZeroKgSets_AreUnweighted() =>
        Assert.That(rule.Best(Definition(PushUp), Sets(PushUp, Set(20, null), Set(25, 0m))), Is.EqualTo(new ExerciseBest(null, 25, null, null)));

    [Test]
    public void Best_TiedWeight_TakesTheMostReps() =>
        Assert.That(rule.Best(Definition(PushUp), Sets(PushUp, Set(8, 10m), Set(12, 10m))),
            Is.EqualTo(new ExerciseBest(10m, 12, null, null)));

    [TestCase(null, 25, null, 20, 1)]
    [TestCase(null, 20, null, 25, -1)]
    [TestCase(10, 5, null, 30, 1)]
    [TestCase(10, 6, 10, 5, 1)]
    [TestCase(null, 20, null, 20, 0)]
    public void Compare_HeavierWins_UnweightedIs0Kg_TieMoreReps(double? xKg, int xReps, double? yKg, int yReps, int expected) =>
        Assert.That(Math.Sign(rule.Compare(new ExerciseBest((decimal?)xKg, xReps, null, null), new ExerciseBest((decimal?)yKg, yReps, null, null))),
            Is.EqualTo(expected));
}
