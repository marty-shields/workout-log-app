using System.Globalization;

namespace WorkoutLog.UI.Tests;

public class DisplayFormatTests
{
    [TestCase("22.5", "22.5kg")]
    [TestCase("60.00", "60kg")]
    [TestCase("62.4999999999", "62.5kg")]
    [TestCase("22.505", "22.51kg")]
    public void Weight_RoundedTo2PlacesWithoutTrailingZeros(string kg, string expected) =>
        Assert.That(DisplayFormat.Weight(decimal.Parse(kg, CultureInfo.InvariantCulture)), Is.EqualTo(expected));

    [Test]
    public void Weight_None_IsADash() => Assert.That(DisplayFormat.Weight(null), Is.EqualTo("—"));

    [Test]
    public void ShortDate_ThisYear_HasNoYear() =>
        Assert.That(DisplayFormat.ShortDate(new DateTime(2026, 1, 1, 0, 0, 0), new DateTime(2026, 12, 31, 23, 59, 59)), Is.EqualTo("Thu 1 Jan"));

    [Test]
    public void ShortDate_AnotherYear_HasTheYear() =>
        Assert.That(DisplayFormat.ShortDate(new DateTime(2025, 12, 31, 23, 59, 59), new DateTime(2026, 1, 1, 0, 0, 0)), Is.EqualTo("Wed 31 Dec 2025"));

    [Test]
    public void DayMonth_ThisYear_HasNoYear() =>
        Assert.That(DisplayFormat.DayMonth(new DateTime(2026, 1, 1, 0, 0, 0), new DateTime(2026, 12, 31, 23, 59, 59)), Is.EqualTo("1 Jan"));

    [Test]
    public void DayMonth_AnotherYear_HasTheYear() =>
        Assert.That(DisplayFormat.DayMonth(new DateTime(2025, 12, 31, 23, 59, 59), new DateTime(2026, 1, 1, 0, 0, 0)), Is.EqualTo("31 Dec 2025"));

    [TestCase(0, "Today")]
    [TestCase(1, "Yesterday")]
    [TestCase(2, "2 days ago")]
    [TestCase(30, "30 days ago")]
    public void DaysAgo_TodayYesterdayOrDays(int days, string expected) =>
        Assert.That(DisplayFormat.DaysAgo(days), Is.EqualTo(expected));

    [TestCase(0, 0, 5, "0:05")]
    [TestCase(0, 1, 45, "1:45")]
    [TestCase(0, 59, 59, "59:59")]
    [TestCase(1, 0, 0, "1:00:00")]
    [TestCase(1, 2, 30, "1:02:30")]
    public void Clock_MinutesAndSecondsOrHoursFromAnHour(int hours, int minutes, int seconds, string expected) =>
        Assert.That(DisplayFormat.Clock(new TimeSpan(hours, minutes, seconds)), Is.EqualTo(expected));
}
