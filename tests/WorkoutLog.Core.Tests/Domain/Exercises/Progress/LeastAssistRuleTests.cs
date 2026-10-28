using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using static WorkoutLog.Core.Tests.Domain.Exercises.Bests.BestRuleCases;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Progress;

public class LeastAssistRuleTests
{
    private readonly LeastAssistRule rule = new();

    [Test]
    public void LeastAssist_IsTheLowestAssist() =>
        Assert.That(rule.LeastAssist([Set(8, 10m), Set(6, 5m), Set(8, 15m)]), Is.EqualTo(new ExerciseBest(5m, 6, null, null)));

    [Test]
    public void LeastAssist_TiedAssist_TakesTheMostReps() =>
        Assert.That(rule.LeastAssist([Set(4, 5m), Set(6, 5m), Set(5, 5m)]), Is.EqualTo(new ExerciseBest(5m, 6, null, null)));

    [Test]
    public void LeastAssist_BlankAssist_IsZeroKg() =>
        Assert.That(rule.LeastAssist([Set(8, 5m), Set(5, null)]), Is.EqualTo(new ExerciseBest(0m, 5, null, null)));

    [Test]
    public void LeastAssist_BlankAndZeroTied_TakesTheMostReps() =>
        Assert.That(rule.LeastAssist([Set(4, 0m), Set(5, null)]), Is.EqualTo(new ExerciseBest(0m, 5, null, null)));

    [Test]
    public void LeastAssist_NoSets_IsNull() =>
        Assert.That(rule.LeastAssist([]), Is.Null);
}
