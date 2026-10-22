namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>A metric an exercise shows.</summary>
/// <param name="Metric">The metric.</param>
/// <param name="Required">Whether it must be filled to save.</param>
/// <param name="Label">Field label, e.g. Assist (kg) or Damper. Null uses the metric's own label.</param>
public sealed record MetricDefinition(Metric Metric, bool Required, string? Label = null);
