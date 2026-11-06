namespace WorkoutLog.Core.Services;

/// <summary>Reads the saved workouts for the Workouts screen. Days are in the phone's time zone.</summary>
public interface IWorkoutsService
{
    /// <summary>
    /// Returns the workouts that started on <paramref name="from"/> to <paramref name="to"/>, both included, by start time newest first.
    /// A failed read returns <see cref="WorkoutsResult.Failed"/> and no workouts.
    /// </summary>
    Task<WorkoutsResult> GetBetweenAsync(DateOnly from, DateOnly to);

    /// <summary>
    /// Counts every saved workout by the day it started on. A failed read returns <see cref="WorkoutDaysResult.Failed"/> and no days.
    /// </summary>
    Task<WorkoutDaysResult> GetDaysAsync();
}
