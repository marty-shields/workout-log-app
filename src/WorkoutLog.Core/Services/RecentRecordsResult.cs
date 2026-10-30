namespace WorkoutLog.Core.Services;

/// <summary>Outcome of loading the recent records.</summary>
/// <param name="Records">Records, newest first. Empty when loading failed.</param>
/// <param name="Now">The time the 30-day window ends at.</param>
/// <param name="Failed">True when the workouts couldn't be read.</param>
public sealed record RecentRecordsResult(IReadOnlyList<ExerciseRecord> Records, DateTimeOffset Now, bool Failed);
