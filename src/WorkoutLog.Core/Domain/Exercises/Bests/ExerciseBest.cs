namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>The best of one session. Values the best doesn't show are null.</summary>
/// <param name="WeightKg">Weight in kg; assistance on assisted machines.</param>
/// <param name="Reps">Repetitions.</param>
/// <param name="Duration">Duration.</param>
/// <param name="Distance">Distance, in the exercise's unit.</param>
public sealed record ExerciseBest(decimal? WeightKg, int? Reps, TimeSpan? Duration, decimal? Distance);
