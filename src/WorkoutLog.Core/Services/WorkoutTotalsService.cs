using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Consistency;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>
/// Adds up the workouts <see cref="IWorkoutRepository.ListTalliesAsync"/> reads and logs the outcome. Days are turned into UTC bounds
/// in the phone's zone, as <see cref="WorkoutsService"/> does.
/// </summary>
internal sealed partial class WorkoutTotalsService(IWorkoutRepository repository, TimeProvider clock, ILogger<WorkoutTotalsService> logger) : IWorkoutTotalsService
{
    private const int DaysPerWeek = 7;

    /// <inheritdoc />
    public async Task<ReadResult<WorkoutTotals>> GetAsync(DateOnly from, DateOnly to)
    {
        var zone = clock.LocalTimeZone;
        IReadOnlyList<WorkoutTally> tallies;
        try
        {
            tallies = await repository.ListTalliesAsync(LocalWeek.Start(from, zone), LocalWeek.Start(to.AddDays(1), zone).AddTicks(-1));
        }
        catch (Exception exception)
        {
            Log.LoadFailed(logger, exception);
            return new ReadFailed();
        }

        var sets = tallies.Sum(t => t.Sets);
        var time = TimeSpan.FromTicks(tallies.Sum(t => t.FinishedAt > t.StartedAt ? (t.FinishedAt - t.StartedAt).Ticks : 0));
        var days = Math.Max(to.DayNumber - from.DayNumber + 1, DaysPerWeek);
        var totals = new WorkoutTotals(tallies.Count, sets, time, tallies.Count / (days / (double)DaysPerWeek));
        Log.Loaded(logger, totals.Workouts, totals.Sets);
        return totals;
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.WorkoutTotalsLoaded, LogLevel.Debug, "Added up {Workouts} workouts with {Sets} sets")]
        public static partial void Loaded(ILogger logger, int workouts, int sets);

        [LoggerMessage((int)CoreEvent.WorkoutTotalsLoadFailed, LogLevel.Error, "Adding up the workouts failed")]
        public static partial void LoadFailed(ILogger logger, Exception exception);
    }
}
