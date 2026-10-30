namespace WorkoutLog.UI.Models.Dashboard;

/// <summary>Counts shown on the Dashboard.</summary>
/// <param name="ThisWeek">Workouts started this week.</param>
/// <param name="ThisMonth">Workouts started this month.</param>
public sealed record DashboardSummary(int ThisWeek, int ThisMonth);
