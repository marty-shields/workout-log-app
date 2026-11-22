using WorkoutLog.UI.Models.Workouts;

namespace WorkoutLog.UI.Components.Trends;

/// <summary>One week of the Month view's Weeks card.</summary>
/// <param name="Dates">The week's dates cut to the month, e.g. 2 – 8 Nov.</param>
/// <param name="Workouts">Workouts started in those dates.</param>
/// <param name="Time">Total time of those workouts.</param>
/// <param name="Span">The same dates, so tapping the row can open their workouts; null makes the row not tappable.</param>
public sealed record MonthWeek(string Dates, int Workouts, TimeSpan Time, DateSpan? Span = null);
