namespace WorkoutLog.UI.Models.Workouts;

/// <summary>A saved workout in the Workouts list.</summary>
/// <param name="Id">Workout id.</param>
/// <param name="StartedAt">Start time in the phone's time zone.</param>
public sealed record WorkoutsRow(int Id, DateTime StartedAt);
