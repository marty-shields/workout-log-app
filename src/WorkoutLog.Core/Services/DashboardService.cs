using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts.Consistency;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>Reads the Dashboard through <see cref="IWorkoutRepository"/>, counts the week and month in the phone's time zone and logs the outcome.</summary>
internal sealed partial class DashboardService(IWorkoutRepository repository, TimeProvider time, ILogger<DashboardService> logger) : IDashboardService
{
    /// <inheritdoc />
    public async Task<DashboardResult> GetAsync()
    {
        DashboardResult result;
        try
        {
            var now = time.GetUtcNow();
            var zone = time.LocalTimeZone;
            var today = TimeZoneInfo.ConvertTime(now, zone).Date;
            var weekStart = LocalWeek.First(today.AddDays(-(((int)today.DayOfWeek + 6) % 7)), zone);
            var monthStart = LocalWeek.First(new DateTime(today.Year, today.Month, 1), zone);

            var last = await repository.ListAsync(limit: 1);
            var recent = await repository.ListAsync(from: weekStart < monthStart ? weekStart : monthStart, to: now);
            result = new DashboardResult(
                last.Count == 0 ? null : last[0].StartedAt,
                recent.Count(w => w.StartedAt >= weekStart),
                recent.Count(w => w.StartedAt >= monthStart),
                Failed: false);
        }
        catch (Exception exception)
        {
            Log.LoadFailed(logger, exception);
            return new DashboardResult(null, 0, 0, Failed: true);
        }

        Log.Loaded(logger, result.ThisWeek, result.ThisMonth);
        return result;
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.DashboardLoaded, LogLevel.Debug, "Dashboard loaded: {ThisWeek} this week, {ThisMonth} this month")]
        public static partial void Loaded(ILogger logger, int thisWeek, int thisMonth);

        [LoggerMessage((int)CoreEvent.DashboardLoadFailed, LogLevel.Error, "Loading the Dashboard failed")]
        public static partial void LoadFailed(ILogger logger, Exception exception);
    }
}
