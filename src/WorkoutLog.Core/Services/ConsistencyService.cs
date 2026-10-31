using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts.Consistency;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>Reads the last 12 weeks' workouts and every start time up to now through <see cref="IWorkoutRepository"/>, summarises them with the rules and logs the outcome.</summary>
internal sealed partial class ConsistencyService(
    IWorkoutRepository repository,
    IWeekComparisonRule comparisonRule,
    IWeekBarsRule barsRule,
    IStreakRule streakRule,
    TimeProvider time,
    ILogger<ConsistencyService> logger) : IConsistencyService
{
    /// <inheritdoc />
    public async Task<ConsistencyResult> GetAsync()
    {
        ConsistencyResult result;
        int workouts;
        try
        {
            var now = time.GetUtcNow();
            var zone = time.LocalTimeZone;
            var from = LocalWeek.Start(LocalWeek.Monday(now, zone).AddDays(-7 * (LocalWeek.Shown - 1)), zone);
            var tallies = await repository.ListTalliesAsync(from, now);
            var starts = await repository.ListAsync(to: now);
            var weekCounts = barsRule.Count(tallies, now, zone);
            result = new ConsistencyResult(comparisonRule.Compare(tallies, now, zone), weekCounts, streakRule.Streak(starts.Select(w => w.StartedAt), now, zone), Failed: false);
            workouts = tallies.Count;
        }
        catch (Exception exception)
        {
            Log.LoadFailed(logger, exception);
            return ConsistencyResult.FailedRead;
        }

        Log.Loaded(logger, workouts, result.Streak);
        return result;
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.ConsistencyLoaded, LogLevel.Information, "Consistency loaded: {Workouts} workouts in the last 12 weeks, {Streak}-week streak")]
        public static partial void Loaded(ILogger logger, int workouts, int streak);

        [LoggerMessage((int)CoreEvent.ConsistencyLoadFailed, LogLevel.Warning, "Loading the Consistency card failed")]
        public static partial void LoadFailed(ILogger logger, Exception exception);
    }
}
