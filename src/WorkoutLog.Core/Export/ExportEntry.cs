namespace WorkoutLog.Core.Export;

/// <summary>A cardio entry. Metrics without a value are null.</summary>
/// <param name="Duration">Duration.</param>
/// <param name="Distance">Distance, in the exercise's unit.</param>
/// <param name="Floors">Floors climbed.</param>
/// <param name="Level">Resistance, damper or speed level.</param>
/// <param name="Incline">Incline, percent.</param>
/// <param name="Calories">Calories.</param>
/// <param name="Rate">Rate, in the exercise's unit.</param>
internal sealed record ExportEntry(
    TimeSpan? Duration, decimal? Distance, int? Floors, int? Level, decimal? Incline, int? Calories, decimal? Rate);
