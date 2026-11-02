namespace WorkoutLog.UI.Models.Workout;

/// <summary>Where a <see cref="WorkoutEditor"/> save stands.</summary>
public enum SavePhase
{
    /// <summary>No save has run, or the last one failed.</summary>
    Idle,

    Running,

    Succeeded,
}
