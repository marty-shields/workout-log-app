using WorkoutLog.Core.Domain;

namespace WorkoutLog.UI.Models.Exercises;

/// <summary>Text for a <see cref="HistoryRange"/>.</summary>
public static class HistoryRanges
{
    /// <summary>The range as text: last 3 months, last 6 months, last year or all time.</summary>
    public static string Text(HistoryRange range) => range switch
    {
        HistoryRange.ThreeMonths => "last 3 months",
        HistoryRange.SixMonths => "last 6 months",
        HistoryRange.Year => "last year",
        _ => "all time",
    };
}
