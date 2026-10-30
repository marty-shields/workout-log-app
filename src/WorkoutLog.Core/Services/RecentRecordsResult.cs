namespace WorkoutLog.Core.Services;

/// <summary>Outcome of loading the recent records.</summary>
/// <param name="Records">Records, newest first. Empty when loading failed.</param>
/// <param name="Failed">True when the workouts couldn't be read.</param>
public sealed record RecentRecordsResult(IReadOnlyList<ExerciseRecord> Records, bool Failed);
