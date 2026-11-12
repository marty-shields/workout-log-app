using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Services;

/// <summary>Reads the saved workouts for the Workouts screen. Days are in the phone's time zone.</summary>
public interface IWorkoutsService
{
    /// <summary>
    /// Returns the workouts that started on <paramref name="from"/> to <paramref name="to"/>, both included, by start time newest first.
    /// </summary>
    Task<ReadResult<IReadOnlyList<WorkoutSummary>>> GetBetweenAsync(DateOnly from, DateOnly to);

    /// <summary>
    /// Counts every saved workout by the day it started on. Days with none are left out.
    /// </summary>
    Task<ReadResult<IReadOnlyDictionary<DateOnly, int>>> GetDaysAsync();
}
