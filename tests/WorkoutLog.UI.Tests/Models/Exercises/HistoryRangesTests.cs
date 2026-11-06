using WorkoutLog.Core.Domain;
using WorkoutLog.UI.Models.Exercises;

namespace WorkoutLog.UI.Tests.Models.Exercises;

public class HistoryRangesTests
{
    [TestCase(HistoryRange.ThreeMonths, "last 3 months")]
    [TestCase(HistoryRange.SixMonths, "last 6 months")]
    [TestCase(HistoryRange.Year, "last year")]
    [TestCase(HistoryRange.All, "all time")]
    public void Text_IsTheRangeAsWords(HistoryRange range, string expected) =>
        Assert.That(HistoryRanges.Text(range), Is.EqualTo(expected));
}
