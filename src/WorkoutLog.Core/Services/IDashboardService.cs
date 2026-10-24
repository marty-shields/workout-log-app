namespace WorkoutLog.Core.Services;

/// <summary>Reads the last workout and the counts for the Dashboard.</summary>
public interface IDashboardService
{
    /// <summary>
    /// Returns the latest start (FR-32) and the workouts started this week (FR-33) and this month (FR-34),
    /// with the week and month in the phone's time zone (NFR-06). A failed read returns <see cref="DashboardResult.Failed"/>.
    /// </summary>
    Task<DashboardResult> GetAsync();
}
