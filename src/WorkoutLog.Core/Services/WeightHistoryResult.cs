namespace WorkoutLog.Core.Services;

/// <summary>Outcome of loading an exercise's weight history.</summary>
/// <param name="LastKg">Best weight in the last saved workout with the exercise. Null when that workout has none, there are none, or loading failed.</param>
/// <param name="BestKg">Best weight in any saved workout: the personal best (PB). Null when none has a weight, or loading failed.</param>
/// <param name="Failed">True when the weights couldn't be read.</param>
public sealed record WeightHistoryResult(decimal? LastKg, decimal? BestKg, bool Failed);
