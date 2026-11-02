using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;

namespace WorkoutLog.Core.Services;

/// <summary>
/// Starts, checks, opens, saves and deletes workouts. The start comes from <see cref="TimeProvider"/>, and the time a new workout is open from <see cref="IElapsedClock"/>.
/// Null input throws <see cref="ArgumentNullException"/>.
/// </summary>
public interface IWorkoutService
{
    /// <summary>Starts a new workout with the current time as its start.</summary>
    WorkoutDraft StartNew();

    /// <summary>Time since <paramref name="draft"/> started, by <see cref="IElapsedClock"/>; never below 0.</summary>
    TimeSpan Elapsed(WorkoutDraft draft);

    /// <summary>Checks the workout and its note against the save rules.</summary>
    WorkoutErrors Validate(string? note, IReadOnlyList<WorkoutExercise> exercises);

    /// <summary>Checks one strength set. The result holds that set only.</summary>
    ExerciseErrors ValidateSet(int exerciseId, StrengthSet set);

    ExerciseErrors ValidateEntry(int exerciseId, CardioEntry entry);

    /// <summary>
    /// Saves a new workout if it is valid. Its finish is the start plus <see cref="Elapsed"/>, and always after the start.
    /// The workout and exercise notes are saved trimmed; a blank note is saved as none.
    /// Nothing is saved when invalid or when the write fails.
    /// </summary>
    Task<SaveResult> SaveNewAsync(WorkoutDraft draft, string? note, IReadOnlyList<WorkoutExercise> exercises);

    /// <summary>
    /// Returns the saved workout. A missing id returns <see cref="WorkoutLoad.NotFound"/>;
    /// a failed read returns <see cref="WorkoutLoad.Failed"/>.
    /// </summary>
    Task<WorkoutResult> GetAsync(int workoutId);

    /// <summary>
    /// Replaces a saved workout's note and exercises if they are valid. The times are not changed.
    /// The workout and exercise notes are saved trimmed; a blank note is saved as none.
    /// Nothing is saved when invalid or when the write fails, including when the workout doesn't exist.
    /// </summary>
    Task<SaveResult> SaveAsync(int workoutId, string? note, IReadOnlyList<WorkoutExercise> exercises);

    /// <summary>
    /// Deletes a saved workout with its exercises, sets and entries. Returns <see cref="WorkoutDelete.Failed"/>
    /// when the write fails, including when the workout doesn't exist.
    /// </summary>
    Task<WorkoutDelete> DeleteAsync(int workoutId);
}
