using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;

namespace WorkoutLog.Core.Services;

/// <summary>Starts, checks and saves workouts. Times come from <see cref="TimeProvider"/>. Null input throws <see cref="ArgumentNullException"/>.</summary>
public interface IWorkoutService
{
    /// <summary>Starts a new workout with the current time as its start (FR-01).</summary>
    WorkoutDraft StartNew();

    /// <summary>Checks the workout against the save rules (FR-09).</summary>
    WorkoutErrors Validate(IReadOnlyList<WorkoutExercise> exercises);

    /// <summary>Checks one strength set. The result holds that set only.</summary>
    ExerciseErrors ValidateSet(int exerciseId, StrengthSet set);

    /// <summary>Checks a cardio entry.</summary>
    ExerciseErrors ValidateEntry(int exerciseId, CardioEntry entry);

    /// <summary>
    /// Saves a new workout if it is valid, with the current time as its finish (FR-10).
    /// The finish is never before the start. Nothing is saved when invalid or when the write fails.
    /// </summary>
    Task<SaveResult> SaveNewAsync(WorkoutDraft draft, IReadOnlyList<WorkoutExercise> exercises);

    /// <summary>
    /// Replaces a saved workout's exercises if they are valid. The times are not changed (FR-11).
    /// Nothing is saved when invalid or when the write fails, including when the workout doesn't exist.
    /// </summary>
    Task<SaveResult> SaveAsync(int workoutId, IReadOnlyList<WorkoutExercise> exercises);
}
