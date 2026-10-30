namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>A saved workout's times and how many strength sets it has.</summary>
/// <param name="StartedAt">Start time, UTC.</param>
/// <param name="FinishedAt">Finish time, UTC.</param>
/// <param name="Sets">Strength sets. Cardio entries are not counted.</param>
public sealed record WorkoutTally(DateTimeOffset StartedAt, DateTimeOffset FinishedAt, int Sets);
