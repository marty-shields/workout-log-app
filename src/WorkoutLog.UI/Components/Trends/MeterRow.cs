namespace WorkoutLog.UI.Components.Trends;

/// <summary>One row of a bar list.</summary>
/// <param name="Label">Row name, e.g. Bench press or Jan.</param>
/// <param name="Value">Count the bar and number show.</param>
/// <param name="ExerciseId">The exercise the row is for, so tapping it can open it; null for a row that is not one (a month).</param>
public sealed record MeterRow(string Label, int Value, int? ExerciseId = null);
