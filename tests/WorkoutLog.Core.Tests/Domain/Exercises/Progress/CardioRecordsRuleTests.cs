using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Progress;

public class CardioRecordsRuleTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 20, 17, 0, 0, TimeSpan.Zero);

    private readonly CardioRecordsRule rule = new();

    [Test]
    public void Records_NoSessions_AreNone() =>
        Assert.That(rule.Records([]), Is.EqualTo(new CardioRecords(null, null, null, null, null, null)));

    [Test]
    public void Records_TakeTheLongestDistanceAndDurationFastestPaceAndSpeedAndMostFloorsAndCalories()
    {
        var earlier = Start.AddDays(-7);

        var records = rule.Records(
        [
            Session(earlier, new CardioBest(5.2m, TimeSpan.FromMinutes(25), TimeSpan.FromSeconds(330), 9.5m, 40, 300)),
            Session(Start, new CardioBest(4m, TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(360), 10.4m, 50, 410)),
        ]);

        Assert.That(records, Is.EqualTo(new CardioRecords(
            new ProgressRecord<decimal>(5.2m, earlier),
            new ProgressRecord<TimeSpan>(TimeSpan.FromMinutes(30), Start),
            new ProgressRecord<TimeSpan>(TimeSpan.FromSeconds(330), earlier),
            new ProgressRecord<decimal>(10.4m, Start),
            new ProgressRecord<int>(50, Start),
            new ProgressRecord<int>(410, Start))));
    }

    [Test]
    public void Records_TiedAcrossSessions_TakeTheEarliest()
    {
        var best = new CardioBest(5m, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(6), 10m, 40, 300);
        var earlier = Start.AddDays(-7);

        var records = rule.Records([Session(Start, best), Session(earlier, best)]);

        Assert.That(records, Is.EqualTo(new CardioRecords(
            new ProgressRecord<decimal>(5m, earlier),
            new ProgressRecord<TimeSpan>(TimeSpan.FromMinutes(30), earlier),
            new ProgressRecord<TimeSpan>(TimeSpan.FromMinutes(6), earlier),
            new ProgressRecord<decimal>(10m, earlier),
            new ProgressRecord<int>(40, earlier),
            new ProgressRecord<int>(300, earlier))));
    }

    [Test]
    public void Records_ValueNotLogged_IsNone() =>
        Assert.That(rule.Records([Session(Start, new CardioBest(null, TimeSpan.FromMinutes(10), null, null, null, null))]),
            Is.EqualTo(new CardioRecords(null, new ProgressRecord<TimeSpan>(TimeSpan.FromMinutes(10), Start), null, null, null, null)));

    [Test]
    public void Records_PaceAndSpeed_AreComparedUnrounded()
    {
        var earlier = Start.AddDays(-1);

        var records = rule.Records(
        [
            Session(earlier, new CardioBest(null, null, TimeSpan.FromTicks(3_000_000_001), 10.001m, null, null)),
            Session(Start, new CardioBest(null, null, TimeSpan.FromTicks(3_000_000_000), 10.002m, null, null)),
        ]);

        Assert.Multiple(() =>
        {
            Assert.That(records.FastestPace, Is.EqualTo(new ProgressRecord<TimeSpan>(TimeSpan.FromTicks(3_000_000_000), Start)));
            Assert.That(records.FastestSpeed, Is.EqualTo(new ProgressRecord<decimal>(10.002m, Start)));
        });
    }

    private static CardioSessionProgress Session(DateTimeOffset startedAt, CardioBest best) =>
        new(new CardioSession(1, startedAt, new CardioEntry(null, null, null, null, null, null, null)), best);
}
