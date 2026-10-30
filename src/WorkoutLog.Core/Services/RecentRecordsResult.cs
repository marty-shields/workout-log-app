namespace WorkoutLog.Core.Services;

/// <summary>Outcome of loading the recent records.</summary>
/// <param name="Records">Records, newest first. Empty when loading failed.</param>
/// <param name="Now">When the records were read; the window ends here.</param>
/// <param name="Failed">True when loading failed.</param>
public sealed record RecentRecordsResult(IReadOnlyList<ExerciseRecord> Records, DateTimeOffset Now, bool Failed);
