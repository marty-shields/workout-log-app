namespace WorkoutLog.UI.Components.Trends;

/// <summary>One row of <see cref="MonthWeeks"/>.</summary>
/// <param name="Dates">The week's dates cut to the month, e.g. 2 – 8 Nov.</param>
/// <param name="Workouts">Workouts started in those dates.</param>
/// <param name="Time">Total time of those workouts.</param>
public sealed record MonthWeek(string Dates, int Workouts, TimeSpan Time);
