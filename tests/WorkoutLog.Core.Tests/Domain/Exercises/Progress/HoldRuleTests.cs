using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using static WorkoutLog.Core.Tests.Domain.Exercises.Bests.BestRuleCases;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Progress;

public class HoldRuleTests
{
    private readonly HoldRule rule = new();

    [Test]
    public void Longest_IsTheLongestDuration() =>
        Assert.That(rule.Longest([Hold(60, null), Hold(105, 10m), Hold(80, null)]), Is.EqualTo(TimeSpan.FromSeconds(105)));

    [Test]
    public void Longest_NoSetHasADuration_IsNull() =>
        Assert.That(rule.Longest([Set(null, 10m), Set(null, null)]), Is.Null);

    [Test]
    public void Longest_NoSets_IsNull() =>
        Assert.That(rule.Longest([]), Is.Null);

    [Test]
    public void Heaviest_IsTheHeaviestSetWithItsDuration() =>
        Assert.That(rule.Heaviest([Hold(105, null), Hold(60, 10m), Hold(80, 5m)]), Is.EqualTo(Best(10m, 60)));

    [Test]
    public void Heaviest_TiedWeight_TakesTheLongest() =>
        Assert.That(rule.Heaviest([Hold(45, 10m), Hold(60, 10m), Hold(50, 10m)]), Is.EqualTo(Best(10m, 60)));

    [Test]
    public void Heaviest_ZeroAndBlankWeights_AreIgnored() =>
        Assert.That(rule.Heaviest([Hold(120, 0m), Hold(90, null), Hold(30, 2.5m)]), Is.EqualTo(Best(2.5m, 30)));

    [Test]
    public void Heaviest_NoSetIsOverZeroKg_IsNull() =>
        Assert.That(rule.Heaviest([Hold(120, 0m), Hold(90, null)]), Is.Null);

    private static StrengthSet Hold(int seconds, decimal? weightKg) => Set(null, weightKg, TimeSpan.FromSeconds(seconds));

    private static ExerciseBest Best(decimal kg, int seconds) => new(kg, null, TimeSpan.FromSeconds(seconds), null);
}
