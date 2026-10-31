namespace WorkoutLog.UI.Models.Summary;

/// <summary>An exercise whose best in a workout is a record.</summary>
/// <param name="ExerciseId">Exercise id.</param>
/// <param name="Exercise">Exercise name.</param>
/// <param name="Best">The record, e.g. 160kg × 3.</param>
public sealed record SummaryRecord(int ExerciseId, string Exercise, string Best);
