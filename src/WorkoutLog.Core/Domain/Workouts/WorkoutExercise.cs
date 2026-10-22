namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>An exercise in a workout.</summary>
/// <param name="ExerciseId">Id of the exercise in the fixed list.</param>
public abstract record WorkoutExercise(int ExerciseId);
