namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A record with the start of the session that set it.</summary>
/// <param name="Value">The record.</param>
/// <param name="StartedAt">Start time of that session, UTC.</param>
public sealed record ProgressRecord<T>(T Value, DateTimeOffset StartedAt);
