namespace WorkoutLog.Core.Services;

/// <summary>The records set inside a period.</summary>
/// <param name="Records">Every record, newest first, ties by exercise name; its length is the total. A session that beat an exercise's best again in a later session counts each time.</param>
/// <param name="PerMonth">Year: the records in each month, January first, 12 counts. Empty for a week or month.</param>
public sealed record PeriodRecords(IReadOnlyList<ExerciseRecord> Records, IReadOnlyList<int> PerMonth);
