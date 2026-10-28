using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Services;

/// <summary>Outcome of loading the saved workouts.</summary>
/// <param name="Workouts">Saved workouts, newest first. Empty when loading failed.</param>
/// <param name="Failed">True when the workouts couldn't be read.</param>
public sealed record WorkoutsResult(IReadOnlyList<WorkoutSummary> Workouts, bool Failed);
