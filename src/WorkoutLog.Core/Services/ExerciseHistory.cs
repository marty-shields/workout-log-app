namespace WorkoutLog.Core.Services;

/// <summary>An exercise's weights and last note in the saved workouts.</summary>
/// <param name="LastKg">Best weight in the last saved workout with the exercise. Null when that workout has none, or there are none.</param>
/// <param name="BestKg">Best weight in any saved workout: the personal best (PB). Null when none has a weight.</param>
/// <param name="LastNote">The exercise's note in the last saved workout with it. Null when that workout has none for it, or there are none.</param>
public sealed record ExerciseHistory(decimal? LastKg, decimal? BestKg, string? LastNote);
