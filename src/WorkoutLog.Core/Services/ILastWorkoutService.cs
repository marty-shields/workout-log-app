using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Services;

/// <summary>Reads the last workout for the Dashboard.</summary>
public interface ILastWorkoutService
{
    /// <summary>
    /// Returns the workout with the latest start, with its exercise ids in the workout's order. Ids not in the exercise list are left out.
    /// Null when there are no workouts.
    /// </summary>
    Task<ReadResult<WorkoutOutline?>> GetAsync();
}
