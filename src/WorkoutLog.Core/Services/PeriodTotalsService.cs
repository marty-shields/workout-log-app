using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Consistency;
using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>Reads the workouts a request needs through <see cref="IWorkoutRepository"/>, summarises them with the rules and logs the outcome.</summary>
internal sealed partial class PeriodTotalsService(
    IWorkoutRepository repository,
    PeriodComparisonRule comparisonRule,
    PeriodSeriesRule seriesRule,
    LongestRunRule longestRunRule,
    WeekBarsRule barsRule,
    StreakRule streakRule,
    TimeProvider time,
    ILogger<PeriodTotalsService> logger) : IPeriodTotalsService
{
    /// <inheritdoc />
    public async Task<ReadResult<PeriodComparison>> GetComparisonAsync(Period period) =>
        await ReadAsync(
            async (now, zone) => comparisonRule.Compare(await ListAsync(period.Previous().Start.AddDays(-1), period, now, zone), period, now, zone),
            _ => Log.ComparisonLoaded(logger, period.Kind, period.Start),
            exception => Log.ComparisonLoadFailed(logger, exception, period.Kind, period.Start));

    /// <inheritdoc />
    public async Task<ReadResult<PeriodSeries>> GetSeriesAsync(Period period) =>
        await ReadAsync(
            async (now, zone) => seriesRule.Build(await ListAsync((period.Kind == PeriodKind.Week ? period.Previous() : period).Start.AddDays(-1), period, now, zone), period, now, zone),
            _ => Log.SeriesLoaded(logger, period.Kind, period.Start),
            exception => Log.SeriesLoadFailed(logger, exception, period.Kind, period.Start));

    /// <inheritdoc />
    public async Task<ReadResult<WeekStreaks>> GetStreaksAsync(int year) =>
        await ReadAsync(
            async (now, zone) =>
            {
                var tallies = await repository.ListTalliesAsync(DateTimeOffset.MinValue, now);
                var starts = tallies.Select(w => w.StartedAt).ToList();
                return new WeekStreaks(streakRule.Streak(starts, now, zone), barsRule.Count(tallies, now, zone), longestRunRule.Longest(starts, year, now, zone));
            },
            streaks => Log.StreaksLoaded(logger, year, streaks.Current, streaks.Longest.Weeks),
            exception => Log.StreaksLoadFailed(logger, exception, year));

    /// <inheritdoc />
    public async Task<ReadResult<PeriodLimits>> GetLimitsAsync(PeriodKind kind) =>
        await ReadAsync(
            async (now, zone) =>
            {
                var latest = Period.Containing(kind, DateOnly.FromDateTime(LocalWeek.Local(now, zone)));
                var tallies = await repository.ListTalliesAsync(DateTimeOffset.MinValue, now);
                var earliest = tallies.Count == 0 ? latest : Period.Containing(kind, DateOnly.FromDateTime(LocalWeek.Local(tallies[0].StartedAt, zone)));
                return new PeriodLimits(earliest, latest);
            },
            _ => Log.LimitsLoaded(logger, kind),
            exception => Log.LimitsLoadFailed(logger, exception, kind));

    /// <summary>Workouts started from the start of <paramref name="from"/> to the end of the day after <paramref name="period"/> ends, up to <paramref name="now"/>; the extra days leave room for the zone's offsets.</summary>
    private Task<IReadOnlyList<WorkoutTally>> ListAsync(DateOnly from, Period period, DateTimeOffset now, TimeZoneInfo zone)
    {
        var to = LocalWeek.Start(period.End.AddDays(2), zone);
        return repository.ListTalliesAsync(LocalWeek.Start(from, zone), to < now ? to : now);
    }

    private async Task<ReadResult<T>> ReadAsync<T>(Func<DateTimeOffset, TimeZoneInfo, Task<T>> load, Action<T> logLoaded, Action<Exception> logFailed)
    {
        T result;
        try
        {
            result = await load(time.GetUtcNow(), time.LocalTimeZone);
        }
        catch (Exception exception)
        {
            logFailed(exception);
            return new ReadFailed();
        }

        logLoaded(result);
        return result;
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.PeriodComparisonLoaded, LogLevel.Information, "Period comparison loaded: {Kind} from {Start:O}")]
        public static partial void ComparisonLoaded(ILogger logger, PeriodKind kind, DateOnly start);

        [LoggerMessage((int)CoreEvent.PeriodComparisonLoadFailed, LogLevel.Warning, "Loading the period comparison failed: {Kind} from {Start:O}")]
        public static partial void ComparisonLoadFailed(ILogger logger, Exception exception, PeriodKind kind, DateOnly start);

        [LoggerMessage((int)CoreEvent.PeriodSeriesLoaded, LogLevel.Information, "Period series loaded: {Kind} from {Start:O}")]
        public static partial void SeriesLoaded(ILogger logger, PeriodKind kind, DateOnly start);

        [LoggerMessage((int)CoreEvent.PeriodSeriesLoadFailed, LogLevel.Warning, "Loading the period series failed: {Kind} from {Start:O}")]
        public static partial void SeriesLoadFailed(ILogger logger, Exception exception, PeriodKind kind, DateOnly start);

        [LoggerMessage((int)CoreEvent.PeriodStreaksLoaded, LogLevel.Information, "Streaks loaded for {Year}: {Current}-week streak, longest run {Longest} weeks")]
        public static partial void StreaksLoaded(ILogger logger, int year, int current, int longest);

        [LoggerMessage((int)CoreEvent.PeriodStreaksLoadFailed, LogLevel.Warning, "Loading the streaks for {Year} failed")]
        public static partial void StreaksLoadFailed(ILogger logger, Exception exception, int year);

        [LoggerMessage((int)CoreEvent.PeriodLimitsLoaded, LogLevel.Information, "Period limits loaded: {Kind}")]
        public static partial void LimitsLoaded(ILogger logger, PeriodKind kind);

        [LoggerMessage((int)CoreEvent.PeriodLimitsLoadFailed, LogLevel.Warning, "Loading the period limits failed: {Kind}")]
        public static partial void LimitsLoadFailed(ILogger logger, Exception exception, PeriodKind kind);
    }
}
