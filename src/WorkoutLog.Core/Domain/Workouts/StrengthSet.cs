namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>One set of a strength exercise. Metrics the exercise doesn't show are null.</summary>
/// <param name="Reps">Repetitions.</param>
/// <param name="WeightKg">Weight in kg; assistance on assisted machines. Required to save; 0 is unweighted or no assist.</param>
/// <param name="Duration">Duration.</param>
/// <param name="Distance">Distance, in the exercise's unit.</param>
public sealed record StrengthSet(int? Reps, decimal? WeightKg, TimeSpan? Duration, decimal? Distance);
