using System.Globalization;
using WorkoutLog.Core.Domain.Workouts.Periods;

namespace WorkoutLog.UI.Tests;

public class DisplayFormatTests
{
    [TestCase("22.5", "22.5kg")]
    [TestCase("60.00", "60kg")]
    [TestCase("62.4999999999", "62.5kg")]
    [TestCase("22.505", "22.51kg")]
    public void Weight_RoundedTo2PlacesWithoutTrailingZeros(string kg, string expected) =>
        Assert.That(DisplayFormat.Weight(decimal.Parse(kg, CultureInfo.InvariantCulture)), Is.EqualTo(expected));

    [TestCase(0, "0m")]
    [TestCase(25, "25m")]
    [TestCase(60, "1h 00m")]
    [TestCase(185, "3h 05m")]
    public void Minutes_AreHoursAndMinutesFromAnHour(int minutes, string expected) =>
        Assert.That(DisplayFormat.Minutes(minutes), Is.EqualTo(expected));

    [TestCase(0, 58, 0, "0:58")]
    [TestCase(1, 2, 59, "1:02")]
    [TestCase(10, 0, 0, "10:00")]
    public void HoursMinutes_DropsSeconds(int hours, int minutes, int seconds, string expected) =>
        Assert.That(DisplayFormat.HoursMinutes(new TimeSpan(hours, minutes, seconds)), Is.EqualTo(expected));

    [TestCase(2026, 11, 16, "16 – 22 Nov")]
    [TestCase(2026, 11, 30, "30 Nov – 6 Dec")]
    [TestCase(2026, 12, 28, "28 Dec – 3 Jan")]
    public void WeekDates_NameTheMonthOnceWhenTheWeekStaysInIt(int year, int month, int day, string expected) =>
        Assert.That(DisplayFormat.WeekDates(new DateOnly(year, month, day)), Is.EqualTo(expected));

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

    [TestCase("9840", PeriodKind.Week, "9,840 kg")]
    [TestCase("1234567", PeriodKind.Month, "1,234,567 kg")]
    [TestCase("0", PeriodKind.Week, "0 kg")]
    [TestCase("999.5", PeriodKind.Month, "1,000 kg")]
    [TestCase("999.4", PeriodKind.Month, "999 kg")]
    [TestCase("998.5", PeriodKind.Week, "999 kg")]
    [TestCase("1000.5", PeriodKind.Week, "1,001 kg")]
    public void Volume_WeekOrMonth_IsWholeKilogramsWithThousandsSeparator(string kg, PeriodKind kind, string expected) =>
        Assert.That(DisplayFormat.Volume(decimal.Parse(kg, CultureInfo.InvariantCulture), kind), Is.EqualTo(expected));

    [TestCase("548000", "548 t")]
    [TestCase("2500", "3 t")]
    [TestCase("3500", "4 t")]
    [TestCase("548499", "548 t")]
    [TestCase("548500", "549 t")]
    [TestCase("1234567", "1,235 t")]
    [TestCase("400", "0 t")]
    public void Volume_Year_IsWholeTonnesRoundedHalfAwayFromZero(string kg, string expected) =>
        Assert.That(DisplayFormat.Volume(decimal.Parse(kg, CultureInfo.InvariantCulture), PeriodKind.Year), Is.EqualTo(expected));

    [Test]
    public void Volume_IgnoresTheCurrentCulture()
    {
        var culture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            Assert.That(DisplayFormat.Volume(9840m, PeriodKind.Week), Is.EqualTo("9,840 kg"));
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    [TestCase("10600", "10000", "+6%")]
    [TestCase("8800", "10000", "-12%")]
    [TestCase("10050", "10000", "+1%")]
    [TestCase("9950", "10000", "-1%")]
    [TestCase("10004", "10000", "0%")]
    [TestCase("5", "0", "—")]
    [TestCase("0", "0", "—")]
    [TestCase("700", "700", "—")]
    [TestCase("0", "700", "-100%")]
    public void PercentChange_IsSignedWholePercentOfTheEarlierFigure(string current, string earlier, string expected) =>
        Assert.That(DisplayFormat.PercentChange(decimal.Parse(current, CultureInfo.InvariantCulture), decimal.Parse(earlier, CultureInfo.InvariantCulture)), Is.EqualTo(expected));

    [TestCase(0, "0 sets")]
    [TestCase(1, "1 set")]
    [TestCase(12, "12 sets")]
    public void Sets_IsSingularOnlyForOne(int count, string expected) => Assert.That(DisplayFormat.Sets(count), Is.EqualTo(expected));

    [TestCase(1, "1st")]
    [TestCase(2, "2nd")]
    [TestCase(3, "3rd")]
    [TestCase(4, "4th")]
    [TestCase(11, "11th")]
    [TestCase(12, "12th")]
    [TestCase(13, "13th")]
    [TestCase(18, "18th")]
    [TestCase(21, "21st")]
    [TestCase(22, "22nd")]
    [TestCase(23, "23rd")]
    [TestCase(30, "30th")]
    [TestCase(31, "31st")]
    public void Ordinal_FollowsEnglishSuffixes(int day, string expected) => Assert.That(DisplayFormat.Ordinal(day), Is.EqualTo(expected));

    [Test]
    public void AgainstLastMonth_NamesTheMonthAndTheDayReached() =>
        Assert.That(DisplayFormat.AgainstLastMonth(new DateOnly(2026, 10, 18), 2026), Is.EqualTo("Against October to the 18th"));

    [Test]
    public void AgainstLastMonth_InJanuary_AddsTheYearOfDecember() =>
        Assert.That(DisplayFormat.AgainstLastMonth(new DateOnly(2025, 12, 18), 2026), Is.EqualTo("Against December 2025 to the 18th"));

    [TestCase(2026, 11, 1, 2026, 11, 1, "1 Nov")]
    [TestCase(2026, 11, 2, 2026, 11, 8, "2 – 8 Nov")]
    [TestCase(2026, 11, 16, 2026, 11, 16, "16 Nov")]
    [TestCase(2026, 9, 28, 2026, 9, 30, "28 – 30 Sep")]
    public void MonthWeekDates_GiveTheFirstDayAndTheLastWhenThereAreSeveral(int year, int month, int day, int lastYear, int lastMonth, int lastDay, string expected) =>
        Assert.That(DisplayFormat.MonthWeekDates(new DateOnly(year, month, day), new DateOnly(lastYear, lastMonth, lastDay)), Is.EqualTo(expected));

    [Test]
    public void Year_IsTheNumberWithoutSeparators() =>
        Assert.That(DisplayFormat.Year(2026), Is.EqualTo("2026"));

    [Test]
    public void AgainstLastYear_NamesTheYearAndTheDateReached() =>
        Assert.That(DisplayFormat.AgainstLastYear(new DateOnly(2025, 11, 18)), Is.EqualTo("Against 2025 to 18 Nov"));

    [TestCase(2026, 9, 2026, "Against September")]
    [TestCase(2025, 12, 2026, "Against December 2025")]
    public void AgainstMonthBefore_AddsTheYearOnlyWhenItDiffers(int year, int month, int shownYear, string expected) =>
        Assert.That(DisplayFormat.AgainstMonthBefore(new DateOnly(year, month, 1), shownYear), Is.EqualTo(expected));

    [TestCase(1, "1 exercise")]
    [TestCase(0, "0 exercises")]
    [TestCase(4, "4 exercises")]
    public void Exercises_IsSingularOnlyForOne(int count, string expected) =>
        Assert.That(DisplayFormat.Exercises(count), Is.EqualTo(expected));

    [TestCase("2026-11-18 09:05", "Started today, 9:05 am")]
    [TestCase("2026-11-17 18:12", "Started yesterday, 6:12 pm")]
    [TestCase("2026-11-17 00:00", "Started yesterday, 12:00 am")]
    [TestCase("2026-11-12 12:30", "Started Thu 12 Nov, 12:30 pm")]
    public void Started_NamesTodayYesterdayOrTheDay(string started, string expected) =>
        Assert.That(DisplayFormat.Started(DateTime.Parse(started, CultureInfo.InvariantCulture), new DateTime(2026, 11, 18, 10, 0, 0)), Is.EqualTo(expected));
}
