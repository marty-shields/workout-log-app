namespace WorkoutLog.UI.Components.Trends;

/// <summary>A period's totals in <see cref="TrendTiles"/>.</summary>
/// <param name="Workouts">Workouts started.</param>
/// <param name="Time">Total time of those workouts.</param>
/// <param name="Sets">Strength sets in those workouts.</param>
/// <param name="VolumeKg">Strength volume in kilograms.</param>
public sealed record TrendTotals(int Workouts, TimeSpan Time, int Sets, decimal VolumeKg);
