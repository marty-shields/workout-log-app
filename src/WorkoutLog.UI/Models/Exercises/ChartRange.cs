namespace WorkoutLog.UI.Models.Exercises;

/// <summary>Time the Exercise chart and records cover, back from today.</summary>
public enum ChartRange
{
    /// <summary>Last 3 months.</summary>
    ThreeMonths,

    /// <summary>Last 6 months.</summary>
    SixMonths,

    /// <summary>Last year.</summary>
    Year,

    /// <summary>All time.</summary>
    All,
}
