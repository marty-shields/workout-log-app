using WorkoutLog.Core.Domain;

namespace WorkoutLog.Core.Tests.Domain;

public class HistoryRangeStartTests
{
    private static readonly DateOnly Today = new(2026, 11, 6);

    [TestCase(HistoryRange.ThreeMonths, 2026, 8, 6)]
    [TestCase(HistoryRange.SixMonths, 2026, 5, 6)]
    [TestCase(HistoryRange.Year, 2025, 11, 6)]
    public void From_IsTodayMinusCalendarMonths(HistoryRange range, int year, int month, int day) =>
        Assert.That(range.From(Today), Is.EqualTo(new DateOnly(year, month, day)));

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
        Assert.That(HistoryRange.All.From(Today), Is.Null);

    [TestCase(HistoryRange.ThreeMonths)]
    [TestCase(HistoryRange.SixMonths)]
    [TestCase(HistoryRange.Year)]
    public void Includes_TheStartDay_IsTrue(HistoryRange range) =>
        Assert.That(range.Includes(Today, range.From(Today)!.Value), Is.True);

    [TestCase(HistoryRange.ThreeMonths)]
    [TestCase(HistoryRange.SixMonths)]
    [TestCase(HistoryRange.Year)]
    public void Includes_TheDayBeforeTheStart_IsFalse(HistoryRange range) =>
        Assert.That(range.Includes(Today, range.From(Today)!.Value.AddDays(-1)), Is.False);

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
        Assert.That(HistoryRange.All.Includes(Today, new DateOnly(1970, 1, 1)), Is.True);

    [Test]
    public void Includes_EndOfALongMonth_UsesTheClampedStart()
    {
        var today = new DateOnly(2026, 5, 31);
        Assert.Multiple(() =>
        {
            Assert.That(HistoryRange.ThreeMonths.Includes(today, new DateOnly(2026, 2, 28)), Is.True);
            Assert.That(HistoryRange.ThreeMonths.Includes(today, new DateOnly(2026, 2, 27)), Is.False);
        });
    }

    [Test]
    public void Includes_UndefinedRange_Throws() =>
        Assert.Multiple(() =>
        {
            Assert.That(() => ((HistoryRange)99).Includes(Today, Today.AddDays(-1)), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => ((HistoryRange)99).Includes(Today, Today.AddDays(1)), Throws.TypeOf<ArgumentOutOfRangeException>());
        });

    [Test]
    public void From_UndefinedRange_Throws() =>
        Assert.That(() => ((HistoryRange)99).From(Today), Throws.TypeOf<ArgumentOutOfRangeException>().With.Property("ParamName").EqualTo("range"));
}
