using WorkoutLog.UI.Models.Workouts;

namespace WorkoutLog.UI.Screens;

/// <summary>A workout tapped in the Workouts list.</summary>
/// <param name="Dates">The days the list showed, which back returns to.</param>
public readonly record struct WorkoutOpened(int Id, DateSpan Dates);
