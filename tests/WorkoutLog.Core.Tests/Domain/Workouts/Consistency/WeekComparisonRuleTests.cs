using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Consistency;

namespace WorkoutLog.Core.Tests.Domain.Workouts.Consistency;

public class WeekComparisonRuleTests
{
    /// <summary>Wednesday 28 Oct 2026, 18:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 18, 0, 0, TimeSpan.Zero);

    private readonly WeekComparisonRule rule = new();

    [Test]
    public void Compare_ThisWeekIsMondayToNow()
    {
        var monday = new DateTimeOffset(2026, 10, 26, 0, 0, 0, TimeSpan.Zero);

        var comparison = rule.Compare([Workout(monday.AddTicks(-1)), Workout(monday), Workout(Now), Workout(Now.AddTicks(1))], Now, TimeZoneInfo.Utc);

        Assert.That(comparison.ThisWeek.Workouts, Is.EqualTo(2));
    }

    [Test]
    public void Compare_LastWeekIsLastMondayToTheSameDayAndTime()
    {
        var lastMonday = new DateTimeOffset(2026, 10, 19, 0, 0, 0, TimeSpan.Zero);
        var sameTime = new DateTimeOffset(2026, 10, 21, 18, 0, 0, TimeSpan.Zero);

        var comparison = rule.Compare(
            [
                Workout(lastMonday.AddTicks(-1)), Workout(lastMonday), Workout(sameTime), Workout(sameTime.AddTicks(1)),
                Workout(new DateTimeOffset(2026, 10, 25, 9, 0, 0, TimeSpan.Zero)),
            ],
            Now, TimeZoneInfo.Utc);

        Assert.That(comparison.LastWeek.Workouts, Is.EqualTo(2));
    }

    [Test]
    public void Compare_AtMonday0000_EachWeekIsItsFirstInstant()
    {
        var now = new DateTimeOffset(2026, 10, 26, 0, 0, 0, TimeSpan.Zero);
        var lastMonday = now.AddDays(-7);

        var comparison = rule.Compare(
            [Workout(lastMonday.AddTicks(-1)), Workout(lastMonday), Workout(lastMonday.AddTicks(1)), Workout(now.AddTicks(-1)), Workout(now)],
            now, TimeZoneInfo.Utc);

        Assert.That((comparison.ThisWeek.Workouts, comparison.LastWeek.Workouts), Is.EqualTo((1, 1)));
    }

    [Test]
    public void Compare_UsesThePhonesTimeZone()
    {
        var now = new DateTimeOffset(2026, 10, 26, 0, 0, 0, TimeSpan.FromHours(10));

        var comparison = rule.Compare([Workout(now.AddTicks(-1)), Workout(now)], now, ConsistencyZones.Plus10);

        Assert.That((comparison.ThisWeek.Workouts, comparison.LastWeek.Workouts), Is.EqualTo((1, 0)));
    }

    [Test]
    public void Compare_ClocksChangedSinceLastWeek_LastWeekEndsAtTheSameLocalTime()
    {
        var now = new DateTimeOffset(2026, 10, 28, 12, 0, 0, TimeSpan.FromHours(2));
        var sameTime = new DateTimeOffset(2026, 10, 21, 12, 0, 0, TimeSpan.FromHours(1));
        var thisMonday = new DateTimeOffset(2026, 10, 26, 0, 0, 0, TimeSpan.FromHours(2));
        var lastMonday = new DateTimeOffset(2026, 10, 19, 0, 0, 0, TimeSpan.FromHours(1));

        var comparison = rule.Compare(
            [
                Workout(lastMonday.AddTicks(-1)), Workout(lastMonday), Workout(sameTime.AddMinutes(-30)), Workout(sameTime), Workout(sameTime.AddTicks(1)),
                Workout(thisMonday.AddTicks(-1)), Workout(thisMonday),
            ],
            now, ConsistencyZones.ForwardOn25Oct);

        Assert.That((comparison.ThisWeek.Workouts, comparison.LastWeek.Workouts), Is.EqualTo((1, 3)));
    }

    [Test]
    public void Compare_SameTimeLastWeekWasSkipped_LastWeekEndsWhenTheClocksWentForward()
    {
        var now = new DateTimeOffset(2026, 11, 1, 2, 30, 0, TimeSpan.FromHours(2));
        var forward = new DateTimeOffset(2026, 10, 25, 3, 0, 0, TimeSpan.FromHours(2));

        var comparison = rule.Compare([Workout(forward), Workout(forward.AddTicks(1))], now, ConsistencyZones.ForwardOn25Oct);

        Assert.That(comparison.LastWeek.Workouts, Is.EqualTo(1));
    }

    [Test]
    public void Compare_TimeIsTheSumOfFinishMinusStart()
    {
        var comparison = rule.Compare(
            [Workout(Now.AddHours(-5), TimeSpan.FromHours(1)), Workout(Now.AddHours(-3), new TimeSpan(2, 5, 0))], Now, TimeZoneInfo.Utc);

        Assert.That(comparison.ThisWeek.Time, Is.EqualTo(new TimeSpan(3, 5, 0)));
    }

    [Test]
    public void Compare_SetsAreSummed()
    {
        var comparison = rule.Compare([Workout(Now.AddHours(-5), sets: 12), Workout(Now.AddHours(-3), sets: 0), Workout(Now, sets: 7)], Now, TimeZoneInfo.Utc);

        Assert.That(comparison.ThisWeek.Sets, Is.EqualTo(19));
    }

    [Test]
    public void Compare_SameWorkoutsEachWeek_TalliesAreEqual()
    {
        var comparison = rule.Compare(
            [Workout(Now.AddHours(-1), TimeSpan.FromMinutes(50), 16), Workout(Now.AddDays(-7).AddHours(-1), TimeSpan.FromMinutes(50), 16)],
            Now, TimeZoneInfo.Utc);

        Assert.Multiple(() =>
        {
            Assert.That(comparison.ThisWeek, Is.EqualTo(new WeekTally(1, TimeSpan.FromMinutes(50), 16)));
            Assert.That(comparison.LastWeek, Is.EqualTo(comparison.ThisWeek));
        });
    }

    [Test]
    public void Compare_NoWorkouts_TalliesAreZero() =>
        Assert.That(rule.Compare([], Now, TimeZoneInfo.Utc), Is.EqualTo(new WeekComparison(WeekTally.None, WeekTally.None)));

    private static WorkoutTally Workout(DateTimeOffset startedAt, TimeSpan? length = null, int sets = 1) =>
        new(startedAt, startedAt + (length ?? TimeSpan.FromMinutes(45)), sets);
}
