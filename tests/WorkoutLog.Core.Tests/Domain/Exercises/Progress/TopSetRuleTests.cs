using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using static WorkoutLog.Core.Tests.Domain.Exercises.Bests.BestRuleCases;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Progress;

public class TopSetRuleTests
{
    private readonly TopSetRule rule = new();

    [Test]
    public void TopSet_IsTheHeaviestSet() =>
        Assert.That(rule.TopSet([Set(8, 60m), Set(3, 70m), Set(6, 65m)]), Is.EqualTo(new ExerciseBest(70m, 3, null, null)));

    [Test]
    public void TopSet_TiedWeight_TakesTheMostReps() =>
        Assert.That(rule.TopSet([Set(3, 70m), Set(5, 70m), Set(4, 70m)]), Is.EqualTo(new ExerciseBest(70m, 5, null, null)));

    [Test]
    public void TopSet_SetsWithoutWeight_AreIgnored() =>
        Assert.That(rule.TopSet([Set(20, null), Set(5, 60m)]), Is.EqualTo(new ExerciseBest(60m, 5, null, null)));

    [Test]
    public void TopSet_NoSetHasAWeight_IsNull() =>
        Assert.That(rule.TopSet([Set(8, null), Set(10, null)]), Is.Null);

    [Test]
    public void TopSet_NoSets_IsNull() =>
        Assert.That(rule.TopSet([]), Is.Null);
}
