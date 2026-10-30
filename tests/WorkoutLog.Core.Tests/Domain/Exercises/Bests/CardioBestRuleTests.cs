using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using static WorkoutLog.Core.Tests.Domain.Exercises.Bests.BestRuleCases;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Bests;

public class CardioBestRuleTests
{
    private readonly CardioBestRule rule = new(new CardioSessionRule());

    [Test]
    public void Exercises_AreTheOnesNamed() =>
        Assert.That(new[] { Run, JumpRope }.Select(id => Definition(id).Name), Is.EqualTo(new[] { "Outdoor Run", "Jump Rope" }));

    [TestCase(Run, true)]
    [TestCase(JumpRope, true)]
    [TestCase(Squat, false)]
    [TestCase(Plank, false)]
    [TestCase(FarmersWalk, false)]
    public void AppliesTo_CardioOnly(int id, bool expected) =>
        Assert.That(rule.AppliesTo(Definition(id)), Is.EqualTo(expected));

    [Test]
    public void Best_NotACardioEntry_IsNull() =>
        Assert.That(rule.Best(Definition(Run), Sets(Run)), Is.Null);

    [Test]
    public void Best_WithDistance_IsTheDistance() =>
        Assert.That(rule.Best(Definition(Run), Entry(Run, TimeSpan.FromMinutes(30), 5.2m)),
            Is.EqualTo(new ExerciseBest(null, null, null, 5.2m)));

    [Test]
    public void Best_DistanceNotLogged_IsNull() =>
        Assert.That(rule.Best(Definition(Run), Entry(Run, TimeSpan.FromMinutes(30), null)), Is.Null);

    [Test]
    public void Best_ExerciseWithoutDistance_IsTheDuration() =>
        Assert.That(rule.Best(Definition(JumpRope), Entry(JumpRope, TimeSpan.FromMinutes(10), null)),
            Is.EqualTo(new ExerciseBest(null, null, TimeSpan.FromMinutes(10), null)));

    [TestCase(5.2, 5.0, 1)]
    [TestCase(5.0, 5.2, -1)]
    [TestCase(5.2, 5.2, 0)]
    public void Compare_Distance_FartherWins(decimal x, decimal y, int expected) =>
        Assert.That(Math.Sign(rule.Compare(new ExerciseBest(null, null, null, x), new ExerciseBest(null, null, null, y))), Is.EqualTo(expected));

    [TestCase(30, 25, 1)]
    [TestCase(25, 30, -1)]
    [TestCase(30, 30, 0)]
    public void Compare_Duration_LongerWins(int xMinutes, int yMinutes, int expected) =>
        Assert.That(Math.Sign(rule.Compare(
                new ExerciseBest(null, null, TimeSpan.FromMinutes(xMinutes), null), new ExerciseBest(null, null, TimeSpan.FromMinutes(yMinutes), null))),
            Is.EqualTo(expected));
}
