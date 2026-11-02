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

    /// <summary>
    /// Sets the workout's note and replaces its exercises, sets and entries. Times are not changed.
    /// Throws <see cref="KeyNotFoundException"/> if the workout doesn't exist.
    /// </summary>
    Task ReplaceAsync(int workoutId, string? note, IReadOnlyList<WorkoutExercise> exercises);

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
    /// Returns the start, finish and strength set count of each workout started from <paramref name="from"/> to <paramref name="to"/>,
    /// both inclusive, by start time oldest first.
    /// </summary>
    Task<IReadOnlyList<WorkoutTally>> ListTalliesAsync(DateTimeOffset from, DateTimeOffset to);

    /// <summary>
    /// Returns the set weights and note of the exercise in each workout that has it, by start time newest first.
    /// Weights are in set order. <paramref name="before"/>, when given, keeps only workouts that started before it.
    /// </summary>
    Task<IReadOnlyList<WorkoutWeights>> GetWeightsAsync(int exerciseId, DateTimeOffset? before);

    /// <summary>Returns each saved exercise id with its last workout: the one with the latest start that has it, with its sets or entry.</summary>
    Task<IReadOnlyList<LastSession>> GetLastSessionsAsync();

    /// <summary>
    /// Returns the exercise's last workout: the one with the latest start that has it, with its sets or entry. With <paramref name="before"/>, the
    /// latest one that started before that time. Null if there is none.
    /// </summary>
    Task<LastSession?> GetLastSessionOfAsync(int exerciseId, DateTimeOffset? before);

    /// <summary>
    /// Returns each workout that has the exercise, by start time oldest first, with the exercise's sets or entry.
    /// </summary>
    Task<IReadOnlyList<WorkoutSession>> GetSessionsAsync(int exerciseId);

    /// <summary>
    /// Returns the number of strength sets of each workout exercise that has any, in workouts started from <paramref name="from"/> to
    /// <paramref name="to"/>, both inclusive, by start time then position.
    /// </summary>
    Task<IReadOnlyList<ExerciseSetCount>> GetSetCountsAsync(DateTimeOffset from, DateTimeOffset to);

    /// <summary>Deletes the workout and its exercises, sets and entries. Throws <see cref="KeyNotFoundException"/> if the workout doesn't exist.</summary>
    Task DeleteAsync(int id);
}
