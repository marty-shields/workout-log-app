namespace WorkoutLog.Core.Services;

/// <summary>How opening a saved workout ended.</summary>
public enum WorkoutLoad
{
    /// <summary>The workout was read.</summary>
    Loaded,

    /// <summary>No workout has the id.</summary>
    NotFound,

    /// <summary>The workout couldn't be read.</summary>
    Failed,
}
