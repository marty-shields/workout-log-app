namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A cardio exercise's records in a range. Each is null when no session has the value.</summary>
/// <param name="LongestDistance">Longest distance, in the exercise's unit.</param>
public sealed record CardioRecords(
    ProgressRecord<decimal>? LongestDistance,
    ProgressRecord<TimeSpan>? LongestDuration,
    ProgressRecord<TimeSpan>? FastestPace,
    ProgressRecord<decimal>? FastestSpeed,
    ProgressRecord<int>? MostFloors,
    ProgressRecord<int>? MostCalories);
