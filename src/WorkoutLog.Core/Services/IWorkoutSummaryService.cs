namespace WorkoutLog.Core.Services;

/// <summary>Reads what a saved workout did and what improved, for the Workout Summary.</summary>
public interface IWorkoutSummaryService
{
    /// <summary>
    /// Returns the summary of the workout: its totals, and each exercise's best, whether it is a record and how it compares with the exercise's last
    /// session before the workout. Ids not in the exercise list are left out. A failed load, or a missing workout, returns <see cref="WorkoutSummaryResult.Failed"/>.
    /// </summary>
    Task<WorkoutSummaryResult> GetAsync(int workoutId);
}
