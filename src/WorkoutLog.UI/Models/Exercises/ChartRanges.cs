using WorkoutLog.Core.Domain.Exercises.Progress;

namespace WorkoutLog.UI.Models.Exercises;

/// <summary>Text for a <see cref="ChartRange"/>.</summary>
public static class ChartRanges
{
    /// <summary>The range as text: last 3 months, last 6 months, last year or all time.</summary>
    public static string Text(ChartRange range) => range switch
    {
        ChartRange.ThreeMonths => "last 3 months",
        ChartRange.SixMonths => "last 6 months",
        ChartRange.Year => "last year",
        _ => "all time",
    };
}
