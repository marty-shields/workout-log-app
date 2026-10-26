namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>A saved workout.</summary>
/// <param name="Id">Workout id.</param>
/// <param name="StartedAt">Start time, UTC.</param>
/// <param name="FinishedAt">Finish time, UTC.</param>
/// <param name="Exercises">Exercises, in the order added or last picked (D236).</param>
public sealed record Workout(int Id, DateTimeOffset StartedAt, DateTimeOffset FinishedAt, IReadOnlyList<WorkoutExercise> Exercises);
