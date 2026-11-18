using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.Core.Tests.Domain.Workouts.Consistency;
using static WorkoutLog.Core.Tests.Domain.Workouts.Periods.PeriodTestData;

namespace WorkoutLog.Core.Tests.Domain.Workouts.Periods;

public class PeriodSeriesRuleTests
{
    private readonly PeriodSeriesRule rule = new();

    [Test]
    public void Build_Week_GivesEachDayOfTheWeekAndTheWeekBefore()
    {
        var series = rule.Build(
            [Workout("2026-10-19T08:00", 30), Workout("2026-10-25T23:30", 90), Workout("2026-10-26T07:00", 40), Workout("2026-10-26T18:00", 20), Workout("2026-11-01T10:00")],
            Week(2026, 10, 28), At("2026-10-28T12:00"), TimeZoneInfo.Utc);

        Assert.Multiple(() =>
        {
            Assert.That(series.Buckets.Select(b => b.Workouts), Is.EqualTo(new[] { 2, 0, 0, 0, 0, 0, 1 }));
            Assert.That(series.Buckets[0], Is.EqualTo(new PeriodBucket(new DateOnly(2026, 10, 26), new DateOnly(2026, 10, 26), 2, TimeSpan.FromMinutes(60))));
            Assert.That(series.Earlier.Select(b => (b.Workouts, b.Time)), Is.EqualTo(new[]
            {
                (1, TimeSpan.FromMinutes(30)), (0, TimeSpan.Zero), (0, TimeSpan.Zero), (0, TimeSpan.Zero), (0, TimeSpan.Zero), (0, TimeSpan.Zero), (1, TimeSpan.FromMinutes(90)),
            }));
            Assert.That(series.Weeks, Is.Empty);
            Assert.That(series.WorkoutsPerWeek, Is.Zero);
        });
    }

    [Test]
    public void Build_Month_GivesEachDayAndEachWeekCutToTheMonth()
    {
        var series = rule.Build(
            [Workout("2026-02-01T10:00", 10), Workout("2026-02-02T10:00", 20), Workout("2026-02-08T10:00", 30), Workout("2026-02-28T10:00", 40), Workout("2026-03-01T10:00")],
            Month(2026, 2), At("2026-03-05T12:00"), TimeZoneInfo.Utc);

        Assert.Multiple(() =>
        {
            Assert.That(series.Buckets, Has.Count.EqualTo(28));
            Assert.That(series.Buckets.Select(b => b.Workouts).Sum(), Is.EqualTo(4));
            Assert.That(series.Weeks.Select(w => (w.Start.Day, w.End.Day, w.Workouts, w.Time.TotalMinutes)), Is.EqualTo(new[]
            {
                (1, 1, 1, 10d), (2, 8, 2, 50d), (9, 15, 0, 0d), (16, 22, 0, 0d), (23, 28, 1, 40d),
            }));
            Assert.That(series.Earlier, Is.Empty);
        });
    }

    [Test]
    public void Build_MonthStartingOnMonday_CutsTheLastWeekAtTheMonthEnd()
    {
        var series = rule.Build([], Month(2026, 6), At("2026-07-05T12:00"), TimeZoneInfo.Utc);

        Assert.That((series.Weeks[0].Start.Day, series.Weeks[^1].Start.Day, series.Weeks[^1].End.Day, series.Weeks.Count), Is.EqualTo((1, 29, 30, 5)));
    }

    [Test]
    public void Build_Year_GivesEachMonthAndTheAverageWeeklyWorkoutsSoFar()
    {
        var series = rule.Build(
            [Workout("2026-01-05T10:00", 30), Workout("2026-01-31T10:00", 30), Workout("2026-03-08T10:00", 60), Workout("2026-03-09T10:00"), Workout("2025-12-31T10:00")],
            Year(2026), At("2026-03-08T12:00"), TimeZoneInfo.Utc);

        Assert.Multiple(() =>
        {
            Assert.That(series.Buckets.Select(b => b.Workouts), Is.EqualTo(new[] { 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
            Assert.That(series.Buckets[0].Time, Is.EqualTo(TimeSpan.FromMinutes(60)));
            Assert.That((series.Buckets[1].Start, series.Buckets[1].End), Is.EqualTo((new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28))));
            Assert.That(series.WorkoutsPerWeek, Is.EqualTo(3 * 7d / 67).Within(1e-9));
        });
    }

    [Test]
    public void Build_FinishedYear_AveragesOverTheWholeYear()
    {
        var series = rule.Build([Workout("2025-05-05T10:00"), Workout("2025-06-05T10:00")], Year(2025), At("2026-03-08T12:00"), TimeZoneInfo.Utc);

        Assert.That(series.WorkoutsPerWeek, Is.EqualTo(2 * 7d / 365).Within(1e-9));
    }

    [Test]
    public void Build_YearNotStarted_AveragesZero() =>
        Assert.That(rule.Build([], Year(2027), At("2026-03-08T12:00"), TimeZoneInfo.Utc).WorkoutsPerWeek, Is.Zero);

    [Test]
    public void Build_NoWorkouts_GivesEmptyBuckets() =>
        Assert.That(rule.Build([], Week(2026, 10, 28), At("2026-10-28T12:00"), TimeZoneInfo.Utc).Buckets, Has.All.Property("Workouts").EqualTo(0));

    [Test]
    public void Build_UsesTheLocalDay()
    {
        // 14:30 UTC on Sunday is 00:30 Monday at UTC+10.
        var series = rule.Build([Workout("2026-10-25T14:30")], Week(2026, 10, 28), At("2026-10-28T12:00"), ConsistencyZones.Plus10);

        Assert.That(series.Buckets[0].Workouts, Is.EqualTo(1));
    }

    [Test]
    public void Build_WeekAcrossAClockChange_PutsEachWorkoutOnItsLocalDay()
    {
        // The offset is +2 after 25 Oct 02:00: 21:30 UTC is 23:30 on Sunday the 25th, 22:30 UTC is 00:30 on Monday the 26th.
        var series = rule.Build([Workout("2026-10-25T21:30"), Workout("2026-10-25T22:30")], Week(2026, 10, 28), At("2026-10-28T12:00"), ConsistencyZones.ForwardOn25Oct);

        Assert.That((series.Earlier[6].Workouts, series.Buckets[0].Workouts), Is.EqualTo((1, 1)));
    }
}
