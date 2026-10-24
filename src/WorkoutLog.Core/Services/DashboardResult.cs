namespace WorkoutLog.Core.Services;

/// <summary>Outcome of loading the Dashboard.</summary>
/// <param name="LastStartedAt">Latest workout start, UTC, or null if there are none or loading failed.</param>
/// <param name="ThisWeek">Workouts started from Monday 00:00 to now. 0 when loading failed.</param>
/// <param name="ThisMonth">Workouts started from the 1st at 00:00 to now. 0 when loading failed.</param>
/// <param name="Failed">True when the workouts couldn't be read.</param>
public sealed record DashboardResult(DateTimeOffset? LastStartedAt, int ThisWeek, int ThisMonth, bool Failed);
