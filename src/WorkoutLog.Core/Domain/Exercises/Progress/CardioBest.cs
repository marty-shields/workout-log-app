namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A cardio session's chart values. Null when the entry has no value.</summary>
/// <param name="Distance">Distance, in the exercise's unit.</param>
/// <param name="Duration">Duration.</param>
/// <param name="Pace">Pace, worked out (<see cref="WorkedOut.Pace"/>): duration ÷ distance, time per km on the exercises that show pace.</param>
/// <param name="Speed">Speed, worked out (<see cref="WorkedOut.Speed"/>): distance ÷ hours, km/h on the exercises that show speed.</param>
/// <param name="Floors">Floors.</param>
/// <param name="Calories">Calories.</param>
public sealed record CardioBest(decimal? Distance, TimeSpan? Duration, TimeSpan? Pace, decimal? Speed, int? Floors, int? Calories);
