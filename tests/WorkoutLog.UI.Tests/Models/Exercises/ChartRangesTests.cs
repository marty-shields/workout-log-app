using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.UI.Models.Exercises;

namespace WorkoutLog.UI.Tests.Models.Exercises;

public class ChartRangesTests
{
    [TestCase(ChartRange.ThreeMonths, "last 3 months")]
    [TestCase(ChartRange.SixMonths, "last 6 months")]
    [TestCase(ChartRange.Year, "last year")]
    [TestCase(ChartRange.All, "all time")]
    public void Text_IsTheRangeAsWords(ChartRange range, string expected) =>
        Assert.That(ChartRanges.Text(range), Is.EqualTo(expected));
}
