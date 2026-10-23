namespace WorkoutLog.Core.Services;

/// <summary>How deleting a saved workout ended.</summary>
public enum WorkoutDelete
{
    /// <summary>The workout and its exercises, sets and entries were removed.</summary>
    Deleted,

    /// <summary>Nothing was removed: the write failed or no workout has the id.</summary>
    Failed,
}
