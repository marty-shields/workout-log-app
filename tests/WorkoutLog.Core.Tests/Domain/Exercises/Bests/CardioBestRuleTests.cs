using WorkoutLog.Core.Domain.Exercises.Bests;
using static WorkoutLog.Core.Tests.Domain.Exercises.Bests.BestRuleCases;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Bests;

public class CardioBestRuleTests
{
    private readonly CardioBestRule rule = new();

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
    public void Best_NoEntry_IsNull() =>
        Assert.That(rule.Best(Definition(Run), []), Is.Null);

    [Test]
    public void Best_WithDistance_IsTheDistance() =>
        Assert.That(rule.Best(Definition(Run), [Entry(Run, TimeSpan.FromMinutes(30), 5.2m)]),
            Is.EqualTo(new ExerciseBest(null, null, null, 5.2m)));

    [Test]
    public void Best_DistanceNotLogged_IsNull() =>
        Assert.That(rule.Best(Definition(Run), [Entry(Run, TimeSpan.FromMinutes(30), null)]), Is.Null);

    [Test]
    public void Best_ExerciseWithoutDistance_IsTheDuration() =>
        Assert.That(rule.Best(Definition(JumpRope), [Entry(JumpRope, TimeSpan.FromMinutes(10), null)]),
            Is.EqualTo(new ExerciseBest(null, null, TimeSpan.FromMinutes(10), null)));

    [Test]
    public void Best_ExerciseTwiceInTheWorkout_TakesTheLongestDistance() =>
        Assert.That(rule.Best(Definition(Run), [Entry(Run, TimeSpan.FromMinutes(30), 5.2m), Entry(Run, TimeSpan.FromMinutes(40), 7m)]),
            Is.EqualTo(new ExerciseBest(null, null, null, 7m)));
}
