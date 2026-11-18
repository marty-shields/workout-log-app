using WorkoutLog.Core.Domain.Workouts.Consistency;
using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.Core.Tests.Domain.Workouts.Consistency;
using static WorkoutLog.Core.Tests.Domain.Workouts.Periods.PeriodTestData;

namespace WorkoutLog.Core.Tests.Domain.Workouts.Periods;

public class PeriodComparisonRuleTests
{
    private readonly PeriodComparisonRule rule = new(new WeekComparisonRule());

    [Test]
    public void Compare_WeekHoldingToday_UsesTheSameWeekdayCut()
    {
        var now = At("2026-10-28T18:00");

        var result = rule.Compare(
            [Workout("2026-10-20T08:00", 30, 4), Workout("2026-10-21T23:00", 60, 5), Workout("2026-10-22T01:00"), Workout("2026-10-26T09:00", 45, 7)],
            Week(2026, 10, 28), now, TimeZoneInfo.Utc);

        Assert.That(result, Is.EqualTo(new PeriodComparison(Week(2026, 10, 28),
            new WeekTally(1, TimeSpan.FromMinutes(45), 7),
            new WeekTally(2, TimeSpan.FromMinutes(90), 9))));
    }

    [Test]
    public void Compare_MonthHoldingToday_CutsTheEarlierMonthAtTheSameDay()
    {
        var result = rule.Compare(
            [Workout("2026-03-15T20:00", 30, 2), Workout("2026-03-16T08:00"), Workout("2026-04-02T08:00", 50, 6), Workout("2026-04-15T10:00", 20, 1)],
            Month(2026, 4), At("2026-04-15T12:00"), TimeZoneInfo.Utc);

        Assert.That((result.Totals, result.Earlier), Is.EqualTo((new WeekTally(2, TimeSpan.FromMinutes(70), 7), new WeekTally(1, TimeSpan.FromMinutes(30), 2))));
    }

    [TestCase("2026-03-31T12:00", 2)]
    [TestCase("2026-03-28T12:00", 2)]
    [TestCase("2026-03-27T12:00", 1)]
    public void Compare_31stAgainstAShorterMonth_CutsAtTheLastDay(string now, int expectedEarlier)
    {
        var result = rule.Compare([Workout("2026-02-27T10:00"), Workout("2026-02-28T22:00")], Month(2026, 3), At(now), TimeZoneInfo.Utc);

        Assert.That(result.Earlier.Workouts, Is.EqualTo(expectedEarlier));
    }

    [Test]
    public void Compare_YearHoldingToday_CutsTheEarlierYearAtTheSameDate()
    {
        var result = rule.Compare(
            [Workout("2027-02-28T10:00"), Workout("2027-03-01T10:00"), Workout("2028-01-05T10:00"), Workout("2028-02-29T09:00")],
            Year(2028), At("2028-02-29T12:00"), TimeZoneInfo.Utc);

        Assert.That((result.Totals.Workouts, result.Earlier.Workouts), Is.EqualTo((2, 1)));
    }

    [Test]
    public void Compare_YearOn1March_IncludesTheEarlier1March()
    {
        var result = rule.Compare([Workout("2027-02-28T10:00"), Workout("2027-03-01T10:00"), Workout("2027-03-02T10:00")], Year(2028), At("2028-03-01T12:00"), TimeZoneInfo.Utc);

        Assert.That(result.Earlier.Workouts, Is.EqualTo(2));
    }

    [TestCase(PeriodKind.Week, "2026-10-14")]
    [TestCase(PeriodKind.Month, "2026-09-10")]
    [TestCase(PeriodKind.Year, "2025-06-10")]
    public void Compare_FinishedPeriod_ComparesWholeToWhole(PeriodKind kind, string day)
    {
        var period = Period.Containing(kind, DateOnly.Parse(day));
        var inside = Workout(period.End.ToString("yyyy-MM-dd") + "T23:30", 60, 9);
        var before = Workout(period.Previous().End.ToString("yyyy-MM-dd") + "T10:00", 30, 2);
        var after = Workout(period.Next().Start.ToString("yyyy-MM-dd") + "T00:30");

        var result = rule.Compare([before, inside, after], period, At("2027-01-20T12:00"), TimeZoneInfo.Utc);

        Assert.That((result.Totals, result.Earlier), Is.EqualTo((new WeekTally(1, TimeSpan.FromHours(1), 9), new WeekTally(1, TimeSpan.FromMinutes(30), 2))));
    }

    [Test]
    public void Compare_EmptyPeriod_IsZero() =>
        Assert.That(rule.Compare([Workout("2026-01-05T10:00")], Month(2026, 8), At("2026-10-28T12:00"), TimeZoneInfo.Utc),
            Is.EqualTo(new PeriodComparison(Month(2026, 8), WeekTally.None, WeekTally.None)));

    [Test]
    public void Compare_NoWorkouts_IsZero() =>
        Assert.That(rule.Compare([], Year(2026), At("2026-10-28T12:00"), TimeZoneInfo.Utc), Is.EqualTo(new PeriodComparison(Year(2026), WeekTally.None, WeekTally.None)));

    [Test]
    public void Compare_WorkoutCrossingMidnightAtMonthEnd_IsInTheMonthItStartedWithItsWholeTime()
    {
        var late = Workout("2026-10-31T23:30", 90, 4);

        var october = rule.Compare([late], Month(2026, 10), At("2026-12-01T12:00"), TimeZoneInfo.Utc);
        var november = rule.Compare([late], Month(2026, 11), At("2026-12-01T12:00"), TimeZoneInfo.Utc);

        Assert.That((october.Totals, november.Totals), Is.EqualTo((new WeekTally(1, TimeSpan.FromMinutes(90), 4), WeekTally.None)));
    }

    [Test]
    public void Compare_UsesTheLocalDayAtTheMonthBoundary()
    {
        var workouts = new[] { Workout("2026-10-31T14:30"), Workout("2026-10-31T13:30") };

        var october = rule.Compare(workouts, Month(2026, 10), At("2026-12-01T12:00"), ConsistencyZones.Plus10);

        Assert.That(october.Totals.Workouts, Is.EqualTo(1));
    }

    [Test]
    public void Compare_MonthBoundaryAcrossAClockChange_UsesEachDaysOffset()
    {
        var workouts = new[] { Workout("2026-10-31T21:30"), Workout("2026-10-31T22:30") };

        var october = rule.Compare(workouts, Month(2026, 10), At("2026-12-01T12:00"), ConsistencyZones.ForwardOn25Oct);
        var november = rule.Compare(workouts, Month(2026, 11), At("2026-12-01T12:00"), ConsistencyZones.ForwardOn25Oct);

        Assert.That((october.Totals.Workouts, november.Totals.Workouts), Is.EqualTo((1, 1)));
    }

    [Test]
    public void Compare_MonthHoldingTodayAcrossAClockChange_CutsOnTheLocalDay()
    {
        // Today is 28 Nov, so the earlier month is cut at the end of 28 Oct. After 25 Oct 02:00 the offset is +2: 21:30 UTC is 23:30 on the 28th, 22:30 UTC is 00:30 on the 29th.
        var workouts = new[] { Workout("2026-10-28T21:30"), Workout("2026-10-28T22:30") };

        var result = rule.Compare(workouts, Month(2026, 11), At("2026-11-28T12:00"), ConsistencyZones.ForwardOn25Oct);

        Assert.That(result.Earlier.Workouts, Is.EqualTo(1));
    }

    [Test]
    public void Compare_TodayAfterLocalMidnightButBeforeUtcMidnight_IsInTheLocalMonth()
    {
        var result = rule.Compare([Workout("2026-10-31T15:00")], Month(2026, 11), At("2026-10-31T15:30"), ConsistencyZones.Plus10);

        Assert.That(result.Totals.Workouts, Is.EqualTo(1));
    }

    [Test]
    public void Compare_WeekFinishedAcrossASkippedMidnight_StartsOnMonday()
    {
        // Mon 5 Oct starts at 02:30 UTC in this zone.
        var result = rule.Compare([Workout("2026-10-05T02:20"), Workout("2026-10-05T02:40")], Week(2026, 10, 5), At("2026-11-20T12:00"), ConsistencyZones.MidnightSkippedOn5Oct);

        Assert.That((result.Totals.Workouts, result.Earlier.Workouts), Is.EqualTo((1, 1)));
    }
}
