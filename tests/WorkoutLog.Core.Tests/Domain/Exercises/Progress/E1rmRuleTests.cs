using WorkoutLog.Core.Domain.Exercises.Progress;
using static WorkoutLog.Core.Tests.Domain.Exercises.Bests.BestRuleCases;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Progress;

public class E1rmRuleTests
{
    private readonly E1rmRule rule = new();

    [Test]
    public void Best_IsEpley() =>
        Assert.That(rule.Best([Set(8, 62.5m)]), Is.EqualTo(62.5m * (1 + 8 / 30m)));

    [Test]
    public void Best_OneRep_IsTheWeight() =>
        Assert.That(rule.Best([Set(1, 100m)]), Is.EqualTo(100m));

    [Test]
    public void Best_TwelveReps_IsUsed() =>
        Assert.That(rule.Best([Set(12, 50m)]), Is.EqualTo(50m * (1 + 12 / 30m)));

    [Test]
    public void Best_OverTwelveReps_IsNotUsed() =>
        Assert.That(rule.Best([Set(13, 60m), Set(15, 70m)]), Is.Null);

    [Test]
    public void Best_TakesTheHighestEstimateNotTheHeaviestSet() =>
        Assert.That(rule.Best([Set(8, 62.5m), Set(3, 70m), Set(13, 80m)]), Is.EqualTo(62.5m * (1 + 8 / 30m)));

    [Test]
    public void Best_SetsWithoutWeight_AreNotUsed() =>
        Assert.That(rule.Best([Set(5, null), Set(3, 40m)]), Is.EqualTo(40m * (1 + 3 / 30m)));

    [Test]
    public void Best_NoSetHasAWeight_IsNull() =>
        Assert.That(rule.Best([Set(5, null)]), Is.Null);

    [Test]
    public void Best_SetWithoutReps_IsNotUsed() =>
        Assert.That(rule.Best([Set(null, 60m)]), Is.Null);

    [Test]
    public void Best_NoSets_IsNull() =>
        Assert.That(rule.Best([]), Is.Null);
}
