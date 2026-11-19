using WorkoutLog.UI.Models.Workouts;

namespace WorkoutLog.UI.Components.Trends;

/// <summary>One column of <see cref="TrendBars"/>.</summary>
/// <param name="Label">Short axis label, e.g. Mon or J.</param>
/// <param name="Name">Full name for screen readers, e.g. Monday or January.</param>
/// <param name="Value">Bar height, in the chart's unit.</param>
/// <param name="ValueText">The value in words for screen readers, e.g. 1h 02m or 17 workouts.</param>
/// <param name="Earlier">Height of the paired bar from the period before; 0 draws none.</param>
/// <param name="EarlierText">The earlier value in words, e.g. 1h 05m or rest day.</param>
/// <param name="Current">Marks the column for today or this month: its axis label is bold, and in a single chart its bar is in the accent colour.</param>
/// <param name="Labelled">Prints <paramref name="Value"/> above the bar.</param>
/// <param name="Span">The days the bar counts, so tapping its column can open their workouts; null makes the column not tappable.</param>
public sealed record TrendBar(
    string Label,
    string Name,
    double Value,
    string ValueText,
    double Earlier = 0,
    string? EarlierText = null,
    bool Current = false,
    bool Labelled = false,
    DateSpan? Span = null);
