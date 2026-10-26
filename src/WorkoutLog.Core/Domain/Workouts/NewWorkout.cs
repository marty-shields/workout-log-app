namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>A workout to save for the first time.</summary>
/// <param name="StartedAt">Start time, UTC.</param>
/// <param name="FinishedAt">Finish time, UTC.</param>
/// <param name="Exercises">Exercises, in the order added or last picked (D42).</param>
public sealed record NewWorkout(DateTimeOffset StartedAt, DateTimeOffset FinishedAt, IReadOnlyList<WorkoutExercise> Exercises);
