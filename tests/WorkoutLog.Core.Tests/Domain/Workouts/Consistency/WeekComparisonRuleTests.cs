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
    public void Compare_WorkoutLaterOnTheSameWeekdayLastWeek_IsInLastWeekToDate()
    {
        var lastWednesday = new DateTimeOffset(2026, 10, 21, 0, 0, 0, TimeSpan.Zero);

        var comparison = rule.Compare(
            [Workout(lastWednesday.AddHours(17).AddMinutes(59)), Workout(lastWednesday.AddHours(23).AddMinutes(30)), Workout(lastWednesday.AddDays(1))],
            Now, TimeZoneInfo.Utc);

        Assert.That(comparison.LastWeekToDate.Workouts, Is.EqualTo(2));
    }

    [Test]
    public void Compare_LastWeekToDateIsLastMondayToTheEndOfTheSameWeekday()
    {
        var lastMonday = new DateTimeOffset(2026, 10, 19, 0, 0, 0, TimeSpan.Zero);
        var endOfLastWednesday = lastMonday.AddDays(3);

        var comparison = rule.Compare(
            [Workout(lastMonday.AddTicks(-1)), Workout(lastMonday), Workout(endOfLastWednesday.AddTicks(-1)), Workout(endOfLastWednesday), Workout(lastMonday.AddDays(7).AddTicks(-1))],
            Now, TimeZoneInfo.Utc);

        Assert.That(comparison.LastWeekToDate.Workouts, Is.EqualTo(2));
    }

    [Test]
    public void Compare_WorkoutAfterNowThisWeek_IsNotCountedButOneAfterTheSameTimeLastWeekIs()
    {
        var comparison = rule.Compare([Workout(Now), Workout(Now.AddTicks(1)), Workout(Now.AddDays(-7)), Workout(Now.AddDays(-7).AddTicks(1))], Now, TimeZoneInfo.Utc);

        Assert.That((comparison.ThisWeek.Workouts, comparison.LastWeekToDate.Workouts), Is.EqualTo((1, 2)));
    }

    [Test]
    public void Compare_SaturdayWorkoutOnTuesdayMorning_IsOnTheBarsButNotYetInLastWeekToDate()
    {
        var london = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
        var now = new DateTimeOffset(2026, 11, 3, 5, 12, 0, TimeSpan.Zero);
        var saturday = new DateTimeOffset(2026, 10, 31, 10, 0, 0, TimeSpan.Zero);

        var comparison = rule.Compare([Workout(saturday)], now, london);
        var bars = new WeekBarsRule().Count([Workout(saturday)], now, london);

        Assert.That((comparison.LastWeekToDate.Workouts, bars[^2]), Is.EqualTo((0, 1)));
    }

    [Test]
    public void Compare_AtTheLastInstantOfSunday_LastWeekToDateIsTheSecondLastBar()
    {
        var bars = new WeekBarsRule();
        var start = new DateTimeOffset(2026, 9, 21, 0, 0, 0, TimeSpan.Zero);
        var workouts = Enumerable.Range(0, 24 * 40).Select(hours => Workout(start.AddHours(hours * 1.7))).ToList();

        foreach (var zone in Zones)
        {
            for (var monday = new DateOnly(2026, 10, 5); monday <= new DateOnly(2026, 11, 2); monday = monday.AddDays(7))
            {
                var now = LocalWeek.Start(monday.AddDays(7), zone).AddTicks(-1);

                Assert.That(rule.Compare(workouts, now, zone).LastWeekToDate.Workouts, Is.EqualTo(bars.Count(workouts, now, zone)[^2]), $"{zone.Id} at {now:u}");
            }
        }
    }

    [Test]
    public void Compare_SameWallClockTimesEachWeek_LastWeekIsEveryTimeUpToTheEndOfTheDayAcrossClockChanges()
    {
        var firstMonday = new DateOnly(2026, 9, 28);
        var weeks = Enumerable.Range(0, 6).Select(week => firstMonday.AddDays(7 * week).ToDateTime(TimeOnly.MinValue)).ToList();
        var times = Enumerable.Range(0, 7 * 24 * 60 / 97).Select(step => TimeSpan.FromMinutes(step * 97)).ToList();

        foreach (var zone in Zones)
        {
            var pattern = times.Where(time => weeks.All(week => !zone.IsInvalidTime(week + time))).ToList();
            var workouts = weeks.SelectMany(week => pattern.Select(time => Workout(LocalWeek.First(week + time, zone)))).ToList();

            foreach (var week in weeks.Skip(1))
            {
                for (var local = week; local < week.AddDays(7); local = local.AddMinutes(53))
                {
                    if (zone.IsInvalidTime(local) || zone.IsAmbiguousTime(local))
                    {
                        continue;
                    }

                    var now = LocalWeek.First(local, zone);
                    var endOfDay = TimeSpan.FromDays((local - week).Days + 1);

                    var comparison = rule.Compare(workouts, now, zone);

                    Assert.Multiple(() =>
                    {
                        Assert.That(comparison.ThisWeek.Workouts, Is.EqualTo(pattern.Count(time => time <= local - week)), $"{zone.Id} this week at {local:yyyy-MM-dd HH:mm}");
                        Assert.That(comparison.LastWeekToDate.Workouts, Is.EqualTo(pattern.Count(time => time < endOfDay)), $"{zone.Id} last week at {local:yyyy-MM-dd HH:mm}");
                    });
                }
            }
        }
    }

    [Test]
    public void Compare_AtMonday0000_ThisWeekIsThatInstantAndLastWeekIsAllOfLastMonday()
    {
        var now = new DateTimeOffset(2026, 10, 26, 0, 0, 0, TimeSpan.Zero);
        var lastMonday = now.AddDays(-7);

        var comparison = rule.Compare(
            [Workout(lastMonday.AddTicks(-1)), Workout(lastMonday), Workout(lastMonday.AddTicks(1)), Workout(now.AddTicks(-1)), Workout(now)],
            now, TimeZoneInfo.Utc);

        Assert.That((comparison.ThisWeek.Workouts, comparison.LastWeekToDate.Workouts), Is.EqualTo((1, 2)));
    }

    [Test]
    public void Compare_AfterClocksGoForward_CutsLastWeekAtTheEndOfTheSameWallClockDay()
    {
        var zone = ConsistencyZones.ForwardOn25Oct;
        var now = LocalWeek.First(new DateTime(2026, 10, 28, 18, 0, 0), zone);
        var samePoint = LocalWeek.First(new DateTime(2026, 10, 21, 18, 0, 0), zone);

        var comparison = rule.Compare([Workout(samePoint.AddMinutes(-30)), Workout(samePoint), Workout(samePoint.AddMinutes(30)), Workout(LocalWeek.First(new DateTime(2026, 10, 21, 23, 30, 0), zone)), Workout(LocalWeek.First(new DateTime(2026, 10, 22, 0, 0, 0), zone))], now, zone);

        Assert.That(comparison.LastWeekToDate.Workouts, Is.EqualTo(4));
    }

    [Test]
    public void Compare_UsesThePhonesTimeZone()
    {
        var now = new DateTimeOffset(2026, 10, 26, 0, 0, 0, TimeSpan.FromHours(10));

        var comparison = rule.Compare([Workout(now.AddTicks(-1)), Workout(now), Workout(now.AddDays(-7))], now, ConsistencyZones.Plus10);

        Assert.That((comparison.ThisWeek.Workouts, comparison.LastWeekToDate.Workouts), Is.EqualTo((1, 1)));
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
            Assert.That(comparison.LastWeekToDate, Is.EqualTo(comparison.ThisWeek));
        });
    }

    [Test]
    public void Compare_NoWorkouts_TalliesAreZero() =>
        Assert.That(rule.Compare([], Now, TimeZoneInfo.Utc), Is.EqualTo(new WeekTallies(WeekTally.None, WeekTally.None)));

    private static readonly TimeZoneInfo[] Zones =
        [TimeZoneInfo.Utc, ConsistencyZones.Plus10, ConsistencyZones.ForwardOn25Oct, ConsistencyZones.BackOn25Oct, ConsistencyZones.MidnightSkippedOn5Oct, ConsistencyZones.MidnightTwiceOn26Oct];

    private static WorkoutTally Workout(DateTimeOffset startedAt, TimeSpan? length = null, int sets = 1) =>
        new(startedAt, startedAt + (length ?? TimeSpan.FromMinutes(45)), sets);
}
