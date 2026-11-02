namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>A saved workout without its exercises.</summary>
/// <param name="StartedAt">Start time, UTC.</param>
public sealed record WorkoutSummary(int Id, DateTimeOffset StartedAt);
