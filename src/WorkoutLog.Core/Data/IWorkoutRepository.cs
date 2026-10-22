using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Data;

/// <summary>Stores workouts with their exercises, sets and entries. Each write is one transaction.</summary>
internal interface IWorkoutRepository
{
    /// <summary>Inserts the workout and its exercises, sets and entries. Returns the new workout id.</summary>
    Task<int> AddAsync(NewWorkout workout);

    /// <summary>Replaces the workout's exercises, sets and entries. Times are not changed. Throws <see cref="KeyNotFoundException"/> if the workout doesn't exist.</summary>
    Task ReplaceExercisesAsync(int workoutId, IReadOnlyList<WorkoutExercise> exercises);

    /// <summary>Returns the workout, or null if it doesn't exist.</summary>
    Task<Workout?> GetAsync(int id);

    /// <summary>Deletes the workout and its exercises, sets and entries. Throws <see cref="KeyNotFoundException"/> if the workout doesn't exist.</summary>
    Task DeleteAsync(int id);
}
