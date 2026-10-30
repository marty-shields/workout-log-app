namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A cardio session's chart values. Null when the entry has no value.</summary>
/// <param name="Distance">Distance, in the exercise's unit.</param>
/// <param name="Duration">Duration.</param>
/// <param name="Pace">Duration ÷ distance (<see cref="WorkedOut.Pace"/>).</param>
/// <param name="Speed">Distance ÷ hours (<see cref="WorkedOut.Speed"/>).</param>
/// <param name="Floors">Floors.</param>
/// <param name="Calories">Calories.</param>
public sealed record CardioBest(decimal? Distance, TimeSpan? Duration, TimeSpan? Pace, decimal? Speed, int? Floors, int? Calories);
