namespace WorkoutLog.Core.Services;

/// <summary>Outcome of loading a workout's summary.</summary>
/// <param name="Summary">The summary. Null when loading failed.</param>
/// <param name="Failed">True when loading failed, including when the workout doesn't exist.</param>
public sealed record WorkoutSummaryResult(SavedWorkoutSummary? Summary, bool Failed);
