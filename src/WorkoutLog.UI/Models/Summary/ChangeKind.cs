namespace WorkoutLog.UI.Models.Summary;

/// <summary>How an exercise's best in a workout compares with its last session before it.</summary>
public enum ChangeKind
{
    /// <summary>The exercise has no earlier session.</summary>
    FirstTime,

    /// <summary>The best is the same as last time.</summary>
    Equal,

    /// <summary>The best is better than last time.</summary>
    Up,

    /// <summary>The best is worse than last time.</summary>
    Down,
}
