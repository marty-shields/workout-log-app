namespace WorkoutLog.Core.Services;

/// <summary>How the file's workouts go in.</summary>
public enum ImportMode
{
    /// <summary>Adds the new workouts; never deletes.</summary>
    Add,

    /// <summary>Deletes every workout on the phone, then adds every workout that can be imported.</summary>
    ReplaceAll,
}
