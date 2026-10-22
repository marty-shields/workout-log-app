using WorkoutLog.Core.Domain.Workouts.Validation;

namespace WorkoutLog.Core.Services;

/// <summary>Outcome of saving a workout.</summary>
/// <param name="WorkoutId">Id of the saved workout. Null when nothing was saved.</param>
/// <param name="Errors">Why the workout wasn't saved. Valid when it was.</param>
public sealed record SaveResult(int? WorkoutId, WorkoutErrors Errors);
