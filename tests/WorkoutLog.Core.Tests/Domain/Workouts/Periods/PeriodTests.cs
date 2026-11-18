using WorkoutLog.Core.Domain.Workouts.Periods;

namespace WorkoutLog.Core.Tests.Domain.Workouts.Periods;

public class PeriodTests
{
    [TestCase(PeriodKind.Week, "2026-10-28", "2026-10-26", "2026-11-01")]
    [TestCase(PeriodKind.Week, "2026-10-26", "2026-10-26", "2026-11-01")]
    [TestCase(PeriodKind.Week, "2026-11-01", "2026-10-26", "2026-11-01")]
    [TestCase(PeriodKind.Week, "2026-12-31", "2026-12-28", "2027-01-03")]
    [TestCase(PeriodKind.Month, "2026-10-28", "2026-10-01", "2026-10-31")]
    [TestCase(PeriodKind.Month, "2028-02-10", "2028-02-01", "2028-02-29")]
    [TestCase(PeriodKind.Month, "2026-02-28", "2026-02-01", "2026-02-28")]
    [TestCase(PeriodKind.Year, "2026-10-28", "2026-01-01", "2026-12-31")]
    [TestCase(PeriodKind.Year, "2026-12-31", "2026-01-01", "2026-12-31")]
    public void Containing_FindsTheFirstAndLastDay(PeriodKind kind, string day, string start, string end)
    {
        var period = Period.Containing(kind, DateOnly.Parse(day));

        Assert.That((period.Kind, period.Start, period.End), Is.EqualTo((kind, DateOnly.Parse(start), DateOnly.Parse(end))));
    }

    [Test]
    public void Containing_AnyDayInThePeriod_GivesTheSamePeriod() =>
        Assert.That(Period.Containing(PeriodKind.Month, new DateOnly(2026, 10, 31)), Is.EqualTo(Period.Containing(PeriodKind.Month, new DateOnly(2026, 10, 1))));

    [TestCase(PeriodKind.Week, "2026-10-28", "2026-10-19", "2026-11-02")]
    [TestCase(PeriodKind.Month, "2026-03-15", "2026-02-01", "2026-04-01")]
    [TestCase(PeriodKind.Month, "2026-01-15", "2025-12-01", "2026-02-01")]
    [TestCase(PeriodKind.Year, "2026-06-01", "2025-01-01", "2027-01-01")]
    public void PreviousAndNext_StepByOnePeriod(PeriodKind kind, string day, string previous, string next)
    {
        var period = Period.Containing(kind, DateOnly.Parse(day));

        Assert.That((period.Previous().Start, period.Next().Start, period.Next().Previous()), Is.EqualTo((DateOnly.Parse(previous), DateOnly.Parse(next), period)));
    }

    [TestCase(PeriodKind.Week, "2026-10-28", "2026-10-21")]
    [TestCase(PeriodKind.Month, "2026-03-31", "2026-02-28")]
    [TestCase(PeriodKind.Month, "2026-03-15", "2026-02-15")]
    [TestCase(PeriodKind.Month, "2026-01-31", "2025-12-31")]
    [TestCase(PeriodKind.Year, "2028-02-29", "2027-02-28")]
    [TestCase(PeriodKind.Year, "2026-10-28", "2025-10-28")]
    public void SamePointBefore_KeepsTheWeekdayDayOrDate(PeriodKind kind, string day, string expected) =>
        Assert.That(Period.Containing(kind, DateOnly.Parse(day)).SamePointBefore(DateOnly.Parse(day)), Is.EqualTo(DateOnly.Parse(expected)));

    [Test]
    public void Contains_IncludesBothEndsOnly()
    {
        var week = Period.Containing(PeriodKind.Week, new DateOnly(2026, 10, 28));

        Assert.That(new[] { new DateOnly(2026, 10, 25), new DateOnly(2026, 10, 26), new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 2) }.Select(week.Contains),
            Is.EqualTo(new[] { false, true, true, false }));
    }
}
