namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>An exercise in a workout.</summary>
/// <param name="ExerciseId">Id of the exercise in the fixed list.</param>
/// <param name="Note">The exercise's note in this workout, trimmed; null when none.</param>
public abstract record WorkoutExercise(int ExerciseId, string? Note = null);
