using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;

namespace WorkoutLog.Core.Services;

/// <summary>
/// Starts, checks, opens, saves and deletes workouts. The start comes from <see cref="TimeProvider"/>, and the time a new workout is open from <see cref="IElapsedClock"/>.
/// Null input throws <see cref="ArgumentNullException"/>.
/// </summary>
public interface IWorkoutService
{
    /// <summary>Starts a new workout with the current time as its start (FR-01).</summary>
    WorkoutDraft StartNew();

    /// <summary>Time since <paramref name="draft"/> started, by <see cref="IElapsedClock"/>; never below 0 (FR-02).</summary>
    TimeSpan Elapsed(WorkoutDraft draft);

    /// <summary>Checks the workout against the save rules (FR-09).</summary>
    WorkoutErrors Validate(IReadOnlyList<WorkoutExercise> exercises);

    /// <summary>Checks one strength set. The result holds that set only.</summary>
    ExerciseErrors ValidateSet(int exerciseId, StrengthSet set);

    /// <summary>Checks a cardio entry.</summary>
    ExerciseErrors ValidateEntry(int exerciseId, CardioEntry entry);

    /// <summary>
    /// Saves a new workout if it is valid (FR-10). Its finish is the start plus <see cref="Elapsed"/>, and always after the start.
    /// Nothing is saved when invalid or when the write fails.
    /// </summary>
    Task<SaveResult> SaveNewAsync(WorkoutDraft draft, IReadOnlyList<WorkoutExercise> exercises);

    /// <summary>
    /// Returns the saved workout (FR-21). A missing id returns <see cref="WorkoutLoad.NotFound"/>;
    /// a failed read returns <see cref="WorkoutLoad.Failed"/>.
    /// </summary>
    Task<WorkoutResult> GetAsync(int workoutId);

    /// <summary>
    /// Replaces a saved workout's exercises if they are valid. The times are not changed (FR-11).
    /// Nothing is saved when invalid or when the write fails, including when the workout doesn't exist.
    /// </summary>
    Task<SaveResult> SaveAsync(int workoutId, IReadOnlyList<WorkoutExercise> exercises);

    /// <summary>
    /// Deletes a saved workout with its exercises, sets and entries (FR-24). Returns <see cref="WorkoutDelete.Failed"/>
    /// when the write fails, including when the workout doesn't exist.
    /// </summary>
    Task<WorkoutDelete> DeleteAsync(int workoutId);
}
