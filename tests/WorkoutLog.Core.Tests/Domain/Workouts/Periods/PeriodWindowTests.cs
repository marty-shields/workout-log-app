using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.Core.Tests.Domain.Workouts.Consistency;

namespace WorkoutLog.Core.Tests.Domain.Workouts.Periods;

public class PeriodWindowTests
{
    [TestCase(PeriodKind.Week, "2026-10-28", "2026-10-26", "2026-10-28", "2026-10-19", "2026-10-21")]
    [TestCase(PeriodKind.Week, "2026-10-26", "2026-10-26", "2026-10-26", "2026-10-19", "2026-10-19")]
    [TestCase(PeriodKind.Week, "2026-11-01", "2026-10-26", "2026-11-01", "2026-10-19", "2026-10-25")]
    [TestCase(PeriodKind.Month, "2026-03-31", "2026-03-01", "2026-03-31", "2026-02-01", "2026-02-28")]
    [TestCase(PeriodKind.Month, "2026-10-15", "2026-10-01", "2026-10-15", "2026-09-01", "2026-09-15")]
    [TestCase(PeriodKind.Year, "2028-02-29", "2028-01-01", "2028-02-29", "2027-01-01", "2027-02-28")]
    [TestCase(PeriodKind.Year, "2026-10-28", "2026-01-01", "2026-10-28", "2025-01-01", "2025-10-28")]
    public void Of_PeriodHoldingToday_IsCutAtTodayAndTheEarlierOneAtTheSamePoint(PeriodKind kind, string today, string from, string to, string earlierFrom, string earlierTo)
    {
        var window = PeriodWindow.Of(Period.Containing(kind, DateOnly.Parse(today)), DateOnly.Parse(today));

        Assert.That(window, Is.EqualTo(new PeriodWindow(DateOnly.Parse(from), DateOnly.Parse(to), DateOnly.Parse(earlierFrom), DateOnly.Parse(earlierTo))));
    }

    [TestCase(PeriodKind.Week, "2026-10-28", "2026-11-05", "2026-10-26", "2026-11-01", "2026-10-19", "2026-10-25")]
    [TestCase(PeriodKind.Month, "2026-03-10", "2026-10-05", "2026-03-01", "2026-03-31", "2026-02-01", "2026-02-28")]
    [TestCase(PeriodKind.Year, "2025-06-01", "2026-01-01", "2025-01-01", "2025-12-31", "2024-01-01", "2024-12-31")]
    [TestCase(PeriodKind.Week, "2026-11-04", "2026-10-28", "2026-11-02", "2026-11-08", "2026-10-26", "2026-11-01")]
    public void Of_PeriodNotHoldingToday_IsComparedWholeToWhole(PeriodKind kind, string inPeriod, string today, string from, string to, string earlierFrom, string earlierTo)
    {
        var window = PeriodWindow.Of(Period.Containing(kind, DateOnly.Parse(inPeriod)), DateOnly.Parse(today));

        Assert.That(window, Is.EqualTo(new PeriodWindow(DateOnly.Parse(from), DateOnly.Parse(to), DateOnly.Parse(earlierFrom), DateOnly.Parse(earlierTo))));
    }

    [Test]
    public void IsOn_FirstAndLastLocalDay_AreInAndTheDaysAroundThemAreOut()
    {
        var zone = ConsistencyZones.Plus10;
        var from = new DateOnly(2026, 10, 1);
        var to = new DateOnly(2026, 10, 31);
        var firstMoment = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.FromHours(10));
        var lastMoment = new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.FromHours(10)).AddTicks(-1);

        Assert.Multiple(() =>
        {
            Assert.That(PeriodWindow.IsOn(firstMoment, from, to, zone), Is.True);
            Assert.That(PeriodWindow.IsOn(lastMoment, from, to, zone), Is.True);
            Assert.That(PeriodWindow.IsOn(firstMoment.AddTicks(-1), from, to, zone), Is.False);
            Assert.That(PeriodWindow.IsOn(lastMoment.AddTicks(1), from, to, zone), Is.False);
        });
    }

    [Test]
    public void IsOn_UsesTheLocalDayNotTheUtcDay()
    {
        var utcLateSaturday = new DateTimeOffset(2026, 10, 31, 15, 0, 0, TimeSpan.Zero);

        Assert.Multiple(() =>
        {
            Assert.That(PeriodWindow.IsOn(utcLateSaturday, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 1), ConsistencyZones.Plus10), Is.True);
            Assert.That(PeriodWindow.IsOn(utcLateSaturday, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 1), TimeZoneInfo.Utc), Is.False);
        });
    }

    [Test]
    public void IsOn_ADstZone_PutsAnInstantOnItsLocalDay()
    {
        var zone = ConsistencyZones.ForwardOn25Oct;
        var beforeChange = new DateTimeOffset(2026, 10, 31, 22, 30, 0, TimeSpan.Zero);

        Assert.Multiple(() =>
        {
            Assert.That(PeriodWindow.IsOn(beforeChange, new DateOnly(2026, 10, 31), new DateOnly(2026, 10, 31), zone), Is.False);
            Assert.That(PeriodWindow.IsOn(beforeChange, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 1), zone), Is.True);
        });
    }

    [Test]
    public void IsOn_SingleDayRange_HoldsThatDayOnly()
    {
        var day = new DateOnly(2026, 10, 28);
        var noon = new DateTimeOffset(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

        Assert.Multiple(() =>
        {
            Assert.That(PeriodWindow.IsOn(noon, day, day, TimeZoneInfo.Utc), Is.True);
            Assert.That(PeriodWindow.IsOn(noon.AddDays(1), day, day, TimeZoneInfo.Utc), Is.False);
            Assert.That(PeriodWindow.IsOn(noon, day.AddDays(1), day, TimeZoneInfo.Utc), Is.False, "an empty range holds nothing");
        });
    }
}
