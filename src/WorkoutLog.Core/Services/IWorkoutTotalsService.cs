namespace WorkoutLog.Core.Services;

/// <summary>Adds up the saved workouts of a span for the Workouts screen. Days are in the phone's time zone.</summary>
public interface IWorkoutTotalsService
{
    /// <summary>Returns the totals of the workouts that started on <paramref name="from"/> to <paramref name="to"/>, both included.</summary>
    Task<ReadResult<WorkoutTotals>> GetAsync(DateOnly from, DateOnly to);
}
