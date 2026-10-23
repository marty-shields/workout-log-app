using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Services;

/// <summary>Outcome of loading the history.</summary>
/// <param name="Workouts">Saved workouts, newest first. Empty when loading failed.</param>
/// <param name="Failed">True when the workouts couldn't be read.</param>
public sealed record HistoryResult(IReadOnlyList<WorkoutSummary> Workouts, bool Failed);
