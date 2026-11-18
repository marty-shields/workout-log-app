namespace WorkoutLog.UI.Components.Trends;

/// <summary>One row of <see cref="MeterRows"/>.</summary>
/// <param name="Label">Row name, e.g. Bench press or Jan.</param>
/// <param name="Value">Count the bar and number show.</param>
public sealed record MeterRow(string Label, int Value);
