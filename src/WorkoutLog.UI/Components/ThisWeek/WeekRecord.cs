namespace WorkoutLog.UI.Components.ThisWeek;

/// <summary>One row of a <see cref="WeekRecordsCard"/>.</summary>
/// <param name="Exercise">Exercise name.</param>
/// <param name="Best">The record, e.g. 82.5kg × 5.</param>
/// <param name="Day">Weekday of the session that set it.</param>
/// <param name="When">Text shown under the record in place of the weekday, e.g. Wed 18 Nov for a record in a month. Null shows the weekday.</param>
public sealed record WeekRecord(int ExerciseId, string Exercise, string Best, DayOfWeek Day, string? When = null);
