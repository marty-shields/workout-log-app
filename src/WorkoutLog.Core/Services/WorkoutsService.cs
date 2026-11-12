using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Consistency;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>
/// Reads the saved workouts through <see cref="IWorkoutRepository"/> and logs the outcome. SQLite holds only UTC start times,
/// and the phone's offset changes with daylight saving, so days are turned into UTC bounds, and starts into days, here.
/// </summary>
internal sealed partial class WorkoutsService(IWorkoutRepository repository, TimeProvider clock, ILogger<WorkoutsService> logger) : IWorkoutsService
{
    /// <inheritdoc />
    public async Task<ReadResult<IReadOnlyList<WorkoutSummary>>> GetBetweenAsync(DateOnly from, DateOnly to)
    {
        var zone = clock.LocalTimeZone;
        IReadOnlyList<WorkoutSummary> workouts;
        try
        {
            workouts = await repository.ListAsync(LocalWeek.Start(from, zone), LocalWeek.Start(to.AddDays(1), zone).AddTicks(-1));
        }
        catch (Exception exception)
        {
            Log.LoadFailed(logger, exception);
            return new ReadFailed();
        }

        Log.Loaded(logger, workouts.Count);
        return workouts;
    }

    /// <inheritdoc />
    public async Task<ReadResult<IReadOnlyDictionary<DateOnly, int>>> GetDaysAsync()
    {
        IReadOnlyList<DateTimeOffset> starts;
        try
        {
            starts = await repository.ListStartsAsync();
        }
        catch (Exception exception)
        {
            Log.DaysLoadFailed(logger, exception);
            return new ReadFailed();
        }

        var zone = clock.LocalTimeZone;
        var perDay = starts.CountBy(start => DateOnly.FromDateTime(LocalWeek.Local(start, zone))).ToDictionary();
        Log.DaysLoaded(logger, starts.Count, perDay.Count);
        return perDay;
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.WorkoutsLoaded, LogLevel.Debug, "Loaded {Count} workouts")]
        public static partial void Loaded(ILogger logger, int count);

        [LoggerMessage((int)CoreEvent.WorkoutsLoadFailed, LogLevel.Error, "Loading the workouts failed")]
        public static partial void LoadFailed(ILogger logger, Exception exception);

        [LoggerMessage((int)CoreEvent.WorkoutDaysLoaded, LogLevel.Debug, "Counted {Count} workouts on {Days} days")]
        public static partial void DaysLoaded(ILogger logger, int count, int days);

        [LoggerMessage((int)CoreEvent.WorkoutDaysLoadFailed, LogLevel.Error, "Counting the workouts per day failed")]
        public static partial void DaysLoadFailed(ILogger logger, Exception exception);
    }
}
