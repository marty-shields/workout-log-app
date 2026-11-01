namespace WorkoutLog.Core.Services;

/// <summary>Reads the last workout for the Dashboard.</summary>
public interface ILastWorkoutService
{
    /// <summary>
    /// Returns the workout with the latest start, with its exercise ids in the workout's order. Ids not in the exercise list are left out.
    /// No workouts returns a null <see cref="LastWorkoutResult.Workout"/>. A failed read returns <see cref="LastWorkoutResult.Failed"/>.
    /// </summary>
    Task<LastWorkoutResult> GetAsync();
}
