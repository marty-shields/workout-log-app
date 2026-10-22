namespace WorkoutLog.UI.Models.Dashboard;

/// <summary>Values shown on the Dashboard.</summary>
/// <param name="LastWorkoutStartedAt">Start of the latest workout in the phone's time zone, or null if there are none.</param>
/// <param name="ThisWeek">Workouts started this week.</param>
/// <param name="ThisMonth">Workouts started this month.</param>
public sealed record DashboardSummary(DateTime? LastWorkoutStartedAt, int ThisWeek, int ThisMonth);
