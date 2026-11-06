using System.Globalization;
using WorkoutLog.UI.Models.Workouts;

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

    [TestCase(2026, 8, 6, 2026, 11, 6, "6 Aug – 6 Nov 2026")]
    [TestCase(2025, 3, 1, 2025, 3, 31, "1 Mar – 31 Mar 2025")]
    [TestCase(2025, 11, 6, 2026, 11, 6, "6 Nov 2025 – 6 Nov 2026")]
    [TestCase(2025, 3, 12, 2025, 3, 12, "Wed 12 Mar 2025")]
    public void Dates_DropTheFirstYearWithinAYearAndShowTheDayForOneDay(int fromYear, int fromMonth, int fromDay, int toYear, int toMonth, int toDay, string expected) =>
        Assert.That(DisplayFormat.Dates(new DateSpan(new(fromYear, fromMonth, fromDay), new(toYear, toMonth, toDay))), Is.EqualTo(expected));

    [Test]
    public void DayAndTime_HasNoYear() =>
        Assert.That(DisplayFormat.DayAndTime(new DateTime(2026, 11, 5, 18, 5, 0)), Is.EqualTo("Thu 5 Nov · 18:05"));

    [Test]
    public void Day_HasNoYear() => Assert.That(DisplayFormat.Day(new DateOnly(2025, 3, 12)), Is.EqualTo("Wed 12 Mar"));

    [Test]
    public void DayShortYear_HasTwoYearDigits() => Assert.That(DisplayFormat.DayShortYear(new DateOnly(2025, 3, 12)), Is.EqualTo("Wed 12 Mar 25"));

    [Test]
    public void Month_IsItsNameAndYear() => Assert.That(DisplayFormat.Month(2026, 11), Is.EqualTo("November 2026"));

    [TestCase(0, "0 workouts")]
    [TestCase(1, "1 workout")]
    [TestCase(38, "38 workouts")]
    public void Workouts_IsSingularOnlyForOne(int count, string expected) => Assert.That(DisplayFormat.Workouts(count), Is.EqualTo(expected));
}
