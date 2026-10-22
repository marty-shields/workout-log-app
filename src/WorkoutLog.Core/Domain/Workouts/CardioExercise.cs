namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>A cardio exercise in a workout.</summary>
/// <param name="ExerciseId">Id of the exercise in the fixed list.</param>
/// <param name="Entry">The exercise's one entry.</param>
public sealed record CardioExercise(int ExerciseId, CardioEntry Entry) : WorkoutExercise(ExerciseId);
