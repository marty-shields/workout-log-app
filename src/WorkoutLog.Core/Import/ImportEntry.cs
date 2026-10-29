namespace WorkoutLog.Core.Import;

/// <summary>A cardio entry read from the file. Metrics left out are null.</summary>
/// <param name="Duration">Duration.</param>
/// <param name="Distance">Distance, in the exercise's unit.</param>
/// <param name="Floors">Floors climbed.</param>
/// <param name="Level">Resistance, damper or speed level.</param>
/// <param name="Incline">Incline, percent.</param>
/// <param name="Calories">Calories.</param>
/// <param name="Rate">Rate, in the exercise's unit.</param>
internal sealed record ImportEntry(
    TimeSpan? Duration = null,
    decimal? Distance = null,
    int? Floors = null,
    int? Level = null,
    decimal? Incline = null,
    int? Calories = null,
    decimal? Rate = null);
