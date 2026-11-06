namespace WorkoutLog.Core.Domain;

/// <summary>Time back from today that the Exercise chart and records, and the Workouts list, cover.</summary>
public enum HistoryRange
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
