namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>One strength set of a saved workout, with its workout's start and its exercise.</summary>
/// <param name="StartedAt">The workout's start, UTC.</param>
/// <param name="WeightKg">Null when the set has no weight.</param>
internal sealed record LoggedSet(DateTimeOffset StartedAt, int ExerciseId, int? Reps, decimal? WeightKg);
