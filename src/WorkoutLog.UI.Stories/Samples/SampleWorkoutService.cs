using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary>
/// <see cref="IWorkoutService"/> for the stories. Starts workouts and checks workouts, sets and entries with Core's validator.
/// <see cref="SaveNewAsync"/> stores nothing and returns id 1 when valid; <see cref="SaveAsync"/> throws <see cref="NotSupportedException"/>.
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
    public Task<SaveResult> SaveAsync(int workoutId, IReadOnlyList<WorkoutExercise> exercises) => throw new NotSupportedException();
}
