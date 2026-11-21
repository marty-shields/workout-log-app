using WorkoutLog.UI.Models.Workouts;

namespace WorkoutLog.UI.Components.Trends;

/// <summary>One column of a trend chart.</summary>
/// <param name="Span">The days the bar counts. Its first day dates the bar, and tapping the bar opens the span's workouts.</param>
/// <param name="Label">Short axis label, e.g. Mon or J.</param>
/// <param name="Value">Bar height, in the chart's unit; 0 draws none.</param>
/// <param name="Earlier">Height of the paired bar from the period before; 0 draws none.</param>
/// <param name="Current">Marks the column for today or this month: its axis label is bold, and in a single chart its bar is in the accent colour.</param>
/// <param name="Labelled">Prints <paramref name="Value"/> above the bar.</param>
public sealed record TrendBar(DateSpan Span, string Label, double Value, double Earlier = 0, bool Current = false, bool Labelled = false);
