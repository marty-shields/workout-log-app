using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Data;

/// <summary>Stores workouts with their exercises, sets and entries. Each write is one transaction.</summary>
internal interface IWorkoutRepository
{
    /// <summary>
    /// Inserts the workout and its exercises, sets and entries. Returns the new workout id.
    /// Throws <see cref="SQLite.SQLiteException"/> if a workout with the same start exists.
    /// </summary>
    Task<int> AddAsync(NewWorkout workout);

    /// <summary>Replaces the workout's exercises, sets and entries. Times are not changed. Throws <see cref="KeyNotFoundException"/> if the workout doesn't exist.</summary>
    Task ReplaceExercisesAsync(int workoutId, IReadOnlyList<WorkoutExercise> exercises);

    /// <summary>Returns the workout, or null if it doesn't exist.</summary>
    Task<Workout?> GetAsync(int id);

    /// <summary>Returns every workout with its exercises, by start time oldest first. Exercises and sets are in position order.</summary>
    Task<IReadOnlyList<Workout>> GetAllAsync();

    /// <summary>
    /// Returns workouts without their exercises, by start time newest first.
    /// <paramref name="from"/> and <paramref name="to"/> limit the start time, both inclusive. <paramref name="limit"/> caps the number returned.
    /// </summary>
    Task<IReadOnlyList<WorkoutSummary>> ListAsync(DateTimeOffset? from = null, DateTimeOffset? to = null, int? limit = null);

    /// <summary>Returns the workout with the latest start, with its exercise ids in position order, or null if there are none.</summary>
    Task<WorkoutOutline?> GetLatestAsync();

    /// <summary>
    /// Returns the set weights of each workout that has the exercise, by start time newest first.
    /// Weights are in set order.
    /// </summary>
    Task<IReadOnlyList<WorkoutWeights>> GetWeightsAsync(int exerciseId);

    /// <summary>Returns each saved exercise id with its last workout: the one with the latest start that has it, with its sets or entry.</summary>
    Task<IReadOnlyList<LastSession>> GetLastSessionsAsync();

    /// <summary>
    /// Returns each workout that has the exercise, by start time oldest first, with the exercise's sets or entry.
    /// </summary>
    Task<IReadOnlyList<WorkoutSession>> GetSessionsAsync(int exerciseId);

    /// <summary>Deletes the workout and its exercises, sets and entries. Throws <see cref="KeyNotFoundException"/> if the workout doesn't exist.</summary>
    Task DeleteAsync(int id);
}
