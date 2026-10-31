namespace WorkoutLog.UI.Components.Dashboard;

/// <summary>One row of a <see cref="RecentRecordsCard"/>.</summary>
/// <param name="ExerciseId">Exercise id.</param>
/// <param name="Exercise">Exercise name.</param>
/// <param name="Best">The record, e.g. 160kg × 3.</param>
/// <param name="DaysAgo">Calendar days from the session to today.</param>
public sealed record RecentRecord(int ExerciseId, string Exercise, string Best, int DaysAgo);
