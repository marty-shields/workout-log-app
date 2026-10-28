using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Progress;

public class CardioSessionRuleTests
{
    private readonly CardioSessionRule rule = new();

    [Test]
    public void Best_OneEntry_IsItsValues() =>
        Assert.That(rule.Best([new CardioEntry(TimeSpan.FromMinutes(30), 5.2m, 12, 8, 2m, 410, 85m, TimeSpan.FromSeconds(346))]),
            Is.EqualTo(new CardioBest(5.2m, TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(346), 12, 410)));

    [Test]
    public void Best_TwoEntries_TakesTheLongestDistanceAndDurationFastestPaceAndMostFloorsAndCalories() =>
        Assert.That(rule.Best(
            [
                new CardioEntry(TimeSpan.FromMinutes(40), 3m, 50, null, null, 200, null, TimeSpan.FromMinutes(6)),
                new CardioEntry(TimeSpan.FromMinutes(30), 5.2m, 20, null, null, 410, null, TimeSpan.FromSeconds(346)),
            ]),
            Is.EqualTo(new CardioBest(5.2m, TimeSpan.FromMinutes(40), TimeSpan.FromSeconds(346), 50, 410)));

    [Test]
    public void Best_ValueInOneEntryOnly_IsThatValue() =>
        Assert.That(rule.Best(
            [
                new CardioEntry(TimeSpan.FromMinutes(10), null, null, null, null, null, null, null),
                new CardioEntry(TimeSpan.FromMinutes(5), 1m, null, null, null, 60, null, TimeSpan.FromMinutes(5)),
            ]),
            Is.EqualTo(new CardioBest(1m, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(5), null, 60)));

    [Test]
    public void Best_NoValues_IsNone() =>
        Assert.That(rule.Best([new CardioEntry(null, null, null, null, null, null, null, null)]), Is.EqualTo(new CardioBest(null, null, null, null, null)));

    [Test]
    public void Best_ZeroValues_Count() =>
        Assert.That(rule.Best([new CardioEntry(TimeSpan.Zero, 0m, 0, null, null, 0, null, TimeSpan.Zero)]),
            Is.EqualTo(new CardioBest(0m, TimeSpan.Zero, TimeSpan.Zero, 0, 0)));
}
