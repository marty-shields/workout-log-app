namespace WorkoutLog.Core.Export;

/// <summary>A strength set. Metrics without a value are null.</summary>
/// <param name="Reps">Repetitions.</param>
/// <param name="WeightKg">Weight in kg.</param>
/// <param name="Duration">Duration.</param>
/// <param name="Distance">Distance, in the exercise's unit.</param>
internal sealed record ExportSet(int? Reps, decimal? WeightKg, TimeSpan? Duration, decimal? Distance);
