namespace WorkoutLog.Core.Services;

/// <summary>Outcome of loading an exercise's <see cref="RecordBaseline"/>.</summary>
/// <param name="Baseline">The baseline. Null when loading failed.</param>
/// <param name="Failed">True when the saved sessions couldn't be read.</param>
public sealed record RecordBaselineResult(RecordBaseline? Baseline, bool Failed);
