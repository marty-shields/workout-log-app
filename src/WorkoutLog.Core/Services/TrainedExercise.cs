namespace WorkoutLog.Core.Services;

/// <param name="Sessions">Workouts in the period that include the exercise, cardio too.</param>
public sealed record TrainedExercise(int ExerciseId, int Sessions);
