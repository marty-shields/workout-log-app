using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Consistency;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>
/// Adds up the span's workouts from <see cref="IWorkoutRepository.ListTalliesAsync"/> and logs the outcome. Days become UTC bounds in the
/// phone's zone, as in <see cref="WorkoutsService"/>.
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
        var time = tallies.Aggregate(TimeSpan.Zero, (total, t) => total + (t.FinishedAt > t.StartedAt ? t.FinishedAt - t.StartedAt : TimeSpan.Zero));
        var days = Math.Max(to.DayNumber - from.DayNumber + 1, DaysPerWeek);
        Log.Loaded(logger, tallies.Count, sets);
        return new WorkoutTotals(tallies.Count, sets, time, tallies.Count * (double)DaysPerWeek / days);
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.WorkoutTotalsLoaded, LogLevel.Debug, "Totalled {Workouts} workouts and {Sets} sets")]
        public static partial void Loaded(ILogger logger, int workouts, int sets);

        [LoggerMessage((int)CoreEvent.WorkoutTotalsLoadFailed, LogLevel.Error, "Totalling the workouts failed")]
        public static partial void LoadFailed(ILogger logger, Exception exception);
    }
}
