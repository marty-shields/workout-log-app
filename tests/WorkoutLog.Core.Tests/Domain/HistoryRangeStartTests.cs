using WorkoutLog.Core.Domain;

namespace WorkoutLog.Core.Tests.Domain;

public class HistoryRangeStartTests
{
    [TestCase(HistoryRange.ThreeMonths, 2026, 8, 6)]
    [TestCase(HistoryRange.SixMonths, 2026, 5, 6)]
    [TestCase(HistoryRange.Year, 2025, 11, 6)]
    public void From_IsTodayMinusCalendarMonths(HistoryRange range, int year, int month, int day) =>
        Assert.That(range.From(new DateOnly(2026, 11, 6)), Is.EqualTo(new DateOnly(year, month, day)));

    [TestCase(HistoryRange.ThreeMonths, 2026, 2, 28)]
    [TestCase(HistoryRange.SixMonths, 2025, 11, 30)]
    [TestCase(HistoryRange.Year, 2025, 5, 31)]
    public void From_EndOfALongMonth_IsTheLastDayOfTheShorterMonth(HistoryRange range, int year, int month, int day) =>
        Assert.That(range.From(new DateOnly(2026, 5, 31)), Is.EqualTo(new DateOnly(year, month, day)));

    [Test]
    public void From_LeapDay_IsTheLastDayOfFebruaryAYearBack() =>
        Assert.That(HistoryRange.Year.From(new DateOnly(2028, 2, 29)), Is.EqualTo(new DateOnly(2027, 2, 28)));

    [Test]
    public void From_All_IsNull() =>
        Assert.That(HistoryRange.All.From(new DateOnly(2026, 11, 6)), Is.Null);

    [Test]
    public void From_UndefinedRange_Throws() =>
        Assert.That(() => ((HistoryRange)99).From(new DateOnly(2026, 11, 6)), Throws.TypeOf<ArgumentOutOfRangeException>().With.Property("ParamName").EqualTo("range"));

    private static readonly DateOnly Today = new(2026, 11, 6);

    [Test]
    public void Includes_TheStartDay_IsTrue() =>
        Assert.That(HistoryRange.ThreeMonths.Includes(Today, new DateOnly(2026, 8, 6)), Is.True);

    [Test]
    public void Includes_TheDayBeforeTheStart_IsFalse() =>
        Assert.That(HistoryRange.ThreeMonths.Includes(Today, new DateOnly(2026, 8, 5)), Is.False);

    [TestCase(HistoryRange.ThreeMonths)]
    [TestCase(HistoryRange.SixMonths)]
    [TestCase(HistoryRange.Year)]
    [TestCase(HistoryRange.All)]
    public void Includes_Today_IsTrue(HistoryRange range) =>
        Assert.That(range.Includes(Today, Today), Is.True);

    [TestCase(HistoryRange.ThreeMonths)]
    [TestCase(HistoryRange.SixMonths)]
    [TestCase(HistoryRange.Year)]
    [TestCase(HistoryRange.All)]
    public void Includes_Tomorrow_IsFalse(HistoryRange range) =>
        Assert.That(range.Includes(Today, Today.AddDays(1)), Is.False);

    [Test]
    public void Includes_All_IncludesAVeryOldDate() =>
        Assert.That(HistoryRange.All.Includes(Today, new DateOnly(1900, 1, 1)), Is.True);
}
