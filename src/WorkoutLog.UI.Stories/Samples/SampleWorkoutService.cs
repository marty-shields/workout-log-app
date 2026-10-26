using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary>
/// <see cref="IWorkoutService"/> for the stories and the UI tests. Starts workouts and checks workouts, sets and entries with Core's validator.
/// <see cref="GetAsync"/> returns <see cref="SampleData.SavedStrengthWorkout"/> with the asked id.
/// <see cref="SaveNewAsync"/> and <see cref="SaveAsync"/> store nothing and return an id when valid.
/// <see cref="DeleteAsync"/> deletes nothing and returns <see cref="WorkoutDelete.Deleted"/>.
/// </summary>
public sealed class SampleWorkoutService(TimeProvider time, IExerciseCatalog catalog) : IWorkoutService
{
    private readonly WorkoutValidator validator = new(catalog);

    /// <inheritdoc />
    public WorkoutDraft StartNew() => new(time.GetUtcNow());

    /// <inheritdoc />
    public WorkoutErrors Validate(IReadOnlyList<WorkoutExercise> exercises) => validator.Validate(exercises);

    /// <inheritdoc />
    public ExerciseErrors ValidateSet(int exerciseId, StrengthSet set) => validator.Validate(new StrengthExercise(exerciseId, [set]));

    /// <inheritdoc />
    public ExerciseErrors ValidateEntry(int exerciseId, CardioEntry entry) => validator.Validate(new CardioExercise(exerciseId, entry));

    /// <inheritdoc />
    public Task<SaveResult> SaveNewAsync(WorkoutDraft draft, IReadOnlyList<WorkoutExercise> exercises)
    {
        var errors = validator.Validate(exercises);
        return Task.FromResult(new SaveResult(errors.IsValid ? 1 : null, errors));
    }

    /// <inheritdoc />
    public Task<WorkoutResult> GetAsync(int workoutId) =>
        Task.FromResult(new WorkoutResult(SampleData.SavedStrengthWorkout with { Id = workoutId }, WorkoutLoad.Loaded));

    /// <inheritdoc />
    public Task<SaveResult> SaveAsync(int workoutId, IReadOnlyList<WorkoutExercise> exercises)
    {
        var errors = validator.Validate(exercises);
        return Task.FromResult(new SaveResult(errors.IsValid ? workoutId : null, errors));
    }

    /// <inheritdoc />
    public Task<WorkoutDelete> DeleteAsync(int workoutId) => Task.FromResult(WorkoutDelete.Deleted);
}
