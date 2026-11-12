using WorkoutLog.Core.Domain.Workouts.Validation;

namespace WorkoutLog.Core.Services;

public sealed record Saved(int WorkoutId);

/// <summary>A failed write returns <see cref="WorkoutErrors"/> holding <see cref="WorkoutError.SaveFailed"/>.</summary>
public union SaveResult(Saved, WorkoutErrors);
