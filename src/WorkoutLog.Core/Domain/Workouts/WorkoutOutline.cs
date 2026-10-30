namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>A saved workout's times and exercise ids, without sets or entries.</summary>
/// <param name="Id">Workout id.</param>
/// <param name="StartedAt">Start time, UTC.</param>
/// <param name="FinishedAt">Finish time, UTC.</param>
/// <param name="ExerciseIds">Exercise ids in the workout's order.</param>
public sealed record WorkoutOutline(int Id, DateTimeOffset StartedAt, DateTimeOffset FinishedAt, IReadOnlyList<int> ExerciseIds);
