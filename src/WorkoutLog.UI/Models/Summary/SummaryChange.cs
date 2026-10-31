namespace WorkoutLog.UI.Models.Summary;

/// <summary>An exercise's best in a workout and how it compares with its last session before it.</summary>
/// <param name="ExerciseId">Exercise id.</param>
/// <param name="Exercise">Exercise name.</param>
/// <param name="Best">The workout's best, e.g. 62.5kg × 8.</param>
/// <param name="Since">The best against last time.</param>
public sealed record SummaryChange(int ExerciseId, string Exercise, string Best, SinceLastTime Since);
