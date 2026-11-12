using WorkoutLog.Core.Domain.Workouts.Validation;

namespace WorkoutLog.Core.Services;

/// <summary>The workout was saved with this id.</summary>
public sealed record Saved(int WorkoutId);

/// <summary>The saved workout, or why nothing was saved: the workout's errors, or <see cref="WorkoutError.SaveFailed"/> when the write failed.</summary>
public union SaveResult(Saved, WorkoutErrors);
