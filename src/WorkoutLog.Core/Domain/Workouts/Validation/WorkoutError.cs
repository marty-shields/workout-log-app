namespace WorkoutLog.Core.Domain.Workouts.Validation;

/// <summary>Why a workout can't be saved.</summary>
public enum WorkoutError
{
    /// <summary>The workout has no exercises.</summary>
    NoExercises,

    /// <summary>The database write failed.</summary>
    SaveFailed,
}
