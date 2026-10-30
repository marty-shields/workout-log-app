namespace WorkoutLog.Core.Services;

/// <summary>Reads the last workout and the counts for the Dashboard.</summary>
public interface IDashboardService
{
    /// <summary>
    /// Returns the latest start and the workouts started this week and this month,
    /// with the week and month in the phone's time zone. A failed read returns <see cref="DashboardResult.Failed"/>.
    /// </summary>
    Task<DashboardResult> GetAsync();
}
