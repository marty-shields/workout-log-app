namespace WorkoutLog.UI.Components.Dashboard;

/// <summary>One week's totals in a <see cref="WeekComparison"/>.</summary>
/// <param name="Workouts">Workouts started.</param>
/// <param name="Time">Total time of those workouts.</param>
/// <param name="Sets">Strength sets in those workouts.</param>
public sealed record WeekTotals(int Workouts, TimeSpan Time, int Sets);
