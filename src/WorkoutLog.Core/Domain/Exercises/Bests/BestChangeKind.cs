namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>How a session's best compares with an earlier one.</summary>
public enum BestChangeKind
{
    /// <summary>There is no earlier best.</summary>
    FirstTime,

    /// <summary>The bests tie.</summary>
    Equal,

    /// <summary>The best is better; for an assisted exercise, less assist.</summary>
    Up,

    /// <summary>The best is worse.</summary>
    Down,
}
