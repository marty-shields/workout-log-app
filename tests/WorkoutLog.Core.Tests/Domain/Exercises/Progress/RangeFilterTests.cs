using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Exercises.Progress;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Progress;

public class RangeFilterTests
{
    private static readonly TimeZoneInfo Plus10 = TimeZoneInfo.CreateCustomTimeZone("UTC+10", TimeSpan.FromHours(10), "UTC+10", "UTC+10");

    [TestCase(ChartRange.ThreeMonths, 2026, 7, 28)]
    [TestCase(ChartRange.SixMonths, 2026, 4, 28)]
    [TestCase(ChartRange.Year, 2025, 10, 28)]
    public void Includes_FirstDayOfTheRange_IsIn(ChartRange range, int year, int month, int day)
    {
        var filter = Filter(new DateTimeOffset(2026, 10, 28, 9, 0, 0, TimeSpan.Zero), TimeZoneInfo.Utc);
        var first = new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);

        Assert.Multiple(() =>
        {
            Assert.That(filter.Includes(first, range), Is.True);
            Assert.That(filter.Includes(first.AddTicks(-1), range), Is.False);
        });
    }

    [Test]
    public void Includes_ThreeMonthsFromTheEndOfAMonth_UsesTheLastDayOfTheShorterMonth()
    {
        var filter = Filter(new DateTimeOffset(2026, 5, 31, 12, 0, 0, TimeSpan.Zero), TimeZoneInfo.Utc);

        Assert.Multiple(() =>
        {
            Assert.That(filter.Includes(new DateTimeOffset(2026, 2, 28, 0, 0, 0, TimeSpan.Zero), ChartRange.ThreeMonths), Is.True);
            Assert.That(filter.Includes(new DateTimeOffset(2026, 2, 27, 23, 59, 59, TimeSpan.Zero), ChartRange.ThreeMonths), Is.False);
        });
    }

    [Test]
    public void Includes_UsesThePhonesTimeZoneForTodayAndTheStart()
    {
        // 28 Oct 20:00 UTC is 29 Oct in UTC+10, so the range starts on 29 Jul there.
        var filter = Filter(new DateTimeOffset(2026, 10, 28, 20, 0, 0, TimeSpan.Zero), Plus10);

        Assert.Multiple(() =>
        {
            Assert.That(filter.Includes(new DateTimeOffset(2026, 7, 28, 14, 0, 0, TimeSpan.Zero), ChartRange.ThreeMonths), Is.True);
            Assert.That(filter.Includes(new DateTimeOffset(2026, 7, 28, 13, 59, 59, TimeSpan.Zero), ChartRange.ThreeMonths), Is.False);
        });
    }

    [Test]
    public void Includes_All_IsEverySession()
    {
        var filter = Filter(new DateTimeOffset(2026, 10, 28, 9, 0, 0, TimeSpan.Zero), TimeZoneInfo.Utc);

        Assert.That(filter.Includes(new DateTimeOffset(2001, 1, 1, 0, 0, 0, TimeSpan.Zero), ChartRange.All), Is.True);
    }

    private static RangeFilter Filter(DateTimeOffset now, TimeZoneInfo zone)
    {
        var clock = new FakeTimeProvider(now);
        clock.SetLocalTimeZone(zone);
        return new RangeFilter(clock);
    }
}
