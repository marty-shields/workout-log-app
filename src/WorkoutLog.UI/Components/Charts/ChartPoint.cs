namespace WorkoutLog.UI.Components.Charts;

/// <summary>One point on a <see cref="Chart"/>.</summary>
/// <param name="Date">Date text under the x-axis, e.g. 20 Oct.</param>
/// <param name="Value">Value plotted; none skips the point and the line joins across.</param>
/// <param name="Label">Text over the point, e.g. ×6; the formatted value when unset.</param>
public sealed record ChartPoint(string Date, decimal? Value, string? Label = null);
