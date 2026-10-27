using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Data;

/// <summary>Writes workouts with their exercises, sets and entries on one connection. The caller owns the transaction.</summary>
internal interface IWorkoutWriter
{
    /// <summary>
    /// Inserts the workout and its exercises, sets and entries. Returns the new workout id.
    /// Throws <see cref="SQLite.SQLiteException"/> if a workout with the same start exists.
    /// </summary>
    int Add(NewWorkout workout);

    /// <summary>
    /// Sets the finish time and replaces the exercises, sets and entries of the workout with the same start.
    /// Throws <see cref="KeyNotFoundException"/> if no workout has that start.
    /// </summary>
    void Replace(NewWorkout workout);

    /// <summary>Replaces the workout's exercises, sets and entries. Times are not changed. Throws <see cref="KeyNotFoundException"/> if the workout doesn't exist.</summary>
    void ReplaceExercises(int workoutId, IReadOnlyList<WorkoutExercise> exercises);

    /// <summary>Deletes the workout and its exercises, sets and entries. Throws <see cref="KeyNotFoundException"/> if the workout doesn't exist.</summary>
    void Delete(int workoutId);

    /// <summary>Deletes every workout with its exercises, sets and entries.</summary>
    void DeleteAll();
}
