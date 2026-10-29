using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Progress;

public class CardioSessionRuleTests
{
    private readonly CardioSessionRule rule = new();

    [Test]
    public void Best_IsTheEntrysValuesWithPaceAndSpeedWorkedOut() =>
        Assert.That(rule.Best(new CardioEntry(TimeSpan.FromMinutes(30), 5m, 12, 8, 2m, 410, 85m)),
            Is.EqualTo(new CardioBest(5m, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(6), 10m, 12, 410)));

    [Test]
    public void Best_PaceAndSpeed_AreUnrounded()
    {
        var best = rule.Best(new CardioEntry(TimeSpan.FromMinutes(30), 5.2m, null, null, null, null, null));

        Assert.Multiple(() =>
        {
            Assert.That(best.Pace, Is.EqualTo(TimeSpan.FromTicks(3_461_538_461)));
            Assert.That(best.Speed, Is.EqualTo(10.4m));
        });
    }

    [Test]
    public void Best_NoDistance_HasNoPaceOrSpeed() =>
        Assert.That(rule.Best(new CardioEntry(TimeSpan.FromMinutes(10), null, null, null, null, 60, null)),
            Is.EqualTo(new CardioBest(null, TimeSpan.FromMinutes(10), null, null, null, 60)));

    [Test]
    public void Best_NoValues_IsNone() =>
        Assert.That(rule.Best(new CardioEntry(null, null, null, null, null, null, null)), Is.EqualTo(new CardioBest(null, null, null, null, null, null)));
}
