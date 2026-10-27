namespace WorkoutLog.Core.Import;

/// <summary>A strength set read from the file. Metrics left out are null.</summary>
/// <param name="Reps">Repetitions.</param>
/// <param name="WeightKg">Weight in kg.</param>
/// <param name="Duration">Duration.</param>
/// <param name="Distance">Distance, in the exercise's unit.</param>
internal sealed record ImportSet(int? Reps = null, decimal? WeightKg = null, TimeSpan? Duration = null, decimal? Distance = null);
