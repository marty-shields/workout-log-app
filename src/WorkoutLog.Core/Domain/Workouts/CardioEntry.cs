namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>The entry of a cardio exercise. Metrics the exercise doesn't show are null.</summary>
/// <param name="Duration">Duration. Required to save.</param>
/// <param name="Distance">Distance, in the exercise's unit.</param>
/// <param name="Floors">Floors climbed.</param>
/// <param name="Level">Resistance, damper or speed level.</param>
/// <param name="Incline">Incline, percent.</param>
/// <param name="Calories">Calories.</param>
/// <param name="Rate">Rate, in the exercise's unit.</param>
public sealed record CardioEntry(TimeSpan? Duration, decimal? Distance, int? Floors, int? Level, decimal? Incline, int? Calories, decimal? Rate);
