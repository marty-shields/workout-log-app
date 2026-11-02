namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>The weights and note logged for one exercise in a saved workout.</summary>
/// <param name="WeightsKg">Weight of each set that has one, in kg. Empty when no set has a weight.</param>
/// <param name="Note">The exercise's note in that workout; null when none.</param>
internal sealed record WorkoutWeights(int WorkoutId, IReadOnlyList<decimal> WeightsKg, string? Note);
