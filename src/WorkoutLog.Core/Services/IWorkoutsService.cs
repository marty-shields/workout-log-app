namespace WorkoutLog.Core.Services;

/// <summary>Reads the saved workouts for the Workouts screen.</summary>
public interface IWorkoutsService
{
    /// <summary>
    /// Returns every saved workout, by start time newest first (FR-20).
    /// A failed read returns <see cref="WorkoutsResult.Failed"/> and no workouts.
    /// </summary>
    Task<WorkoutsResult> GetAllAsync();
}
