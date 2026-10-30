namespace WorkoutLog.Core.Domain.Workouts.Validation;

/// <summary>Why a workout can't be saved.</summary>
public enum WorkoutError
{
    /// <summary>The workout has no exercises.</summary>
    NoExercises,

    /// <summary>The database write failed.</summary>
    SaveFailed,

    /// <summary>An exercise appears more than once.</summary>
    RepeatedExercise,

    /// <summary>More than <see cref="WorkoutLimits.MaxExercises"/> exercises.</summary>
    TooManyExercises,
}
