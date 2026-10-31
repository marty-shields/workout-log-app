namespace WorkoutLog.Core.Services;

/// <summary>Outcome of loading an exercise's weight history and last note.</summary>
/// <param name="LastKg">Best weight in the last saved workout with the exercise. Null when that workout has none, there are none, or loading failed.</param>
/// <param name="BestKg">Best weight in any saved workout: the personal best (PB). Null when none has a weight, or loading failed.</param>
/// <param name="LastNote">The exercise's note in the last saved workout with it. Null when that workout has none for it, there are none, or loading failed.</param>
/// <param name="Failed">True when the history couldn't be read.</param>
public sealed record WeightHistoryResult(decimal? LastKg, decimal? BestKg, string? LastNote, bool Failed);
