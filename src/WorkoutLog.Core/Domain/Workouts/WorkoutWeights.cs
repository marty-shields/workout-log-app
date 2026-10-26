namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>The weights logged for one exercise in a saved workout.</summary>
/// <param name="WorkoutId">Workout id.</param>
/// <param name="WeightsKg">Weight of each set that has one, in kg. Empty when no set has a weight.</param>
internal sealed record WorkoutWeights(int WorkoutId, IReadOnlyList<decimal> WeightsKg);
