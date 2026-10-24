using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary><see cref="IDashboardService"/> for the stories: <see cref="SampleData.Started"/> UTC as the last workout, 2 this week and 9 this month.</summary>
public sealed class SampleDashboardService : IDashboardService
{
    /// <inheritdoc />
    public Task<DashboardResult> GetAsync() =>
        Task.FromResult(new DashboardResult(new DateTimeOffset(SampleData.Started, TimeSpan.Zero), 2, 9, Failed: false));
}
