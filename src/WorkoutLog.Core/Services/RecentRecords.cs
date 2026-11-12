namespace WorkoutLog.Core.Services;

/// <summary>The recent records, and when they were read.</summary>
/// <param name="Records">Records, newest first.</param>
/// <param name="Now">When the records were read; the window ends here.</param>
public sealed record RecentRecords(IReadOnlyList<ExerciseRecord> Records, DateTimeOffset Now);
