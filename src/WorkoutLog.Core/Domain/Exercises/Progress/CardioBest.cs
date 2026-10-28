namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A cardio session's best value of each chart metric across its entries. Null when no entry has one.</summary>
/// <param name="Distance">Longest distance, in the exercise's unit.</param>
/// <param name="Duration">Longest duration.</param>
/// <param name="Pace">Fastest pace.</param>
/// <param name="Floors">Most floors.</param>
/// <param name="Calories">Most calories.</param>
public sealed record CardioBest(decimal? Distance, TimeSpan? Duration, TimeSpan? Pace, int? Floors, int? Calories);
