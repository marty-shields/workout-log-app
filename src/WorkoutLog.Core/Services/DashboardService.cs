using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
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
            var weekStart = StartOf(today.AddDays(-(((int)today.DayOfWeek + 6) % 7)), zone);
            var monthStart = StartOf(new DateTime(today.Year, today.Month, 1), zone);

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

    /// <summary>First instant of the date in the zone: the end of the gap if midnight is skipped, the first midnight if it repeats.</summary>
    private static DateTimeOffset StartOf(DateTime date, TimeZoneInfo zone)
    {
        var local = date;
        while (zone.IsInvalidTime(local))
        {
            local = local.AddMinutes(1);
        }

        var offset = zone.IsAmbiguousTime(local) ? zone.GetAmbiguousTimeOffsets(local).Max() : zone.GetUtcOffset(local);
        return new DateTimeOffset(local, offset);
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.DashboardLoaded, LogLevel.Debug, "Dashboard loaded: {ThisWeek} this week, {ThisMonth} this month")]
        public static partial void Loaded(ILogger logger, int thisWeek, int thisMonth);

        [LoggerMessage((int)CoreEvent.DashboardLoadFailed, LogLevel.Error, "Loading the Dashboard failed")]
        public static partial void LoadFailed(ILogger logger, Exception exception);
    }
}
