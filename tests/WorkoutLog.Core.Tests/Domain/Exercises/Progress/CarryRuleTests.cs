using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using static WorkoutLog.Core.Tests.Domain.Exercises.Bests.BestRuleCases;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Progress;

public class CarryRuleTests
{
    private readonly CarryRule rule = new();

    [Test]
    public void Heaviest_IsTheHeaviestSetWithItsDistance() =>
        Assert.That(rule.Heaviest([Carry(28m, 40m), Carry(32m, 30m), Carry(24m, 60m)]), Is.EqualTo(Best(32m, 30m)));

    [Test]
    public void Heaviest_TiedWeight_TakesTheLongest() =>
        Assert.That(rule.Heaviest([Carry(32m, 30m), Carry(32m, 40m), Carry(32m, 35m)]), Is.EqualTo(Best(32m, 40m)));

    [Test]
    public void Heaviest_ZeroAndBlankWeights_AreIgnored() =>
        Assert.That(rule.Heaviest([Carry(0m, 100m), Carry(null, 80m), Carry(10m, 20m)]), Is.EqualTo(Best(10m, 20m)));

    [Test]
    public void Heaviest_NoSetIsOverZeroKg_IsNull() =>
        Assert.That(rule.Heaviest([Carry(0m, 100m), Carry(null, 80m)]), Is.Null);

    [Test]
    public void Longest_IsTheLongestSetWithItsWeight() =>
        Assert.That(rule.Longest([Carry(32m, 40m), Carry(24m, 60m), Carry(28m, 40m)]), Is.EqualTo(Best(24m, 60m)));

    [Test]
    public void Longest_TiedDistance_TakesTheHeaviest() =>
        Assert.That(rule.Longest([Carry(24m, 60m), Carry(28m, 60m), Carry(null, 60m)]), Is.EqualTo(Best(28m, 60m)));

    [TestCase(null)]
    [TestCase(0)]
    public void Longest_UnweightedSet_HasNoWeight(int? kg) =>
        Assert.That(rule.Longest([Carry(kg, 60m), Carry(32m, 40m)]), Is.EqualTo(new ExerciseBest(null, null, null, 60m)));

    [Test]
    public void Longest_NoSetHasADistance_IsNull() =>
        Assert.That(rule.Longest([Carry(32m, null)]), Is.Null);

    [Test]
    public void BothValues_NoSets_AreNull() =>
        Assert.Multiple(() =>
        {
            Assert.That(rule.Heaviest([]), Is.Null);
            Assert.That(rule.Longest([]), Is.Null);
        });

    private static StrengthSet Carry(decimal? weightKg, decimal? metres) => Set(null, weightKg, TimeSpan.FromSeconds(45), metres);

    private static ExerciseBest Best(decimal? kg, decimal metres) => new(kg, null, null, metres);
}
