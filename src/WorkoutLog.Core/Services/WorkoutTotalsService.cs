using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Consistency;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>
/// Reads the workouts' tallies of a span through <see cref="IWorkoutRepository"/>, adds them up and logs the outcome. Days are turned into
/// UTC bounds as <see cref="WorkoutsService"/> does.
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

        var days = Math.Max(to.DayNumber - from.DayNumber + 1, DaysPerWeek);
        var totals = new WorkoutTotals(
            tallies.Count,
            tallies.Sum(t => t.Sets),
            TimeSpan.FromTicks(tallies.Sum(t => Math.Max((t.FinishedAt - t.StartedAt).Ticks, 0))),
            tallies.Count * (double)DaysPerWeek / days);
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
