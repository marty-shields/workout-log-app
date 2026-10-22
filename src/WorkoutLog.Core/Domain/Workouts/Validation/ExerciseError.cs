namespace WorkoutLog.Core.Domain.Workouts.Validation;

/// <summary>Why an exercise in a workout can't be saved.</summary>
public enum ExerciseError
{
    /// <summary>The exercise Id is not in the exercise list.</summary>
    UnknownExercise,

    /// <summary>Strength given for a cardio exercise, or the other way round.</summary>
    WrongType,

    /// <summary>A strength exercise with no sets.</summary>
    NoSets,
}
