using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary>
/// <see cref="IWorkoutService"/> for the stories. Starts workouts and checks strength sets with Core's validator;
/// the other members throw <see cref="NotSupportedException"/>.
/// </summary>
public sealed class SampleWorkoutService(TimeProvider time, IExerciseCatalog catalog) : IWorkoutService
{
    private readonly WorkoutValidator validator = new(catalog);

    /// <inheritdoc />
    public WorkoutDraft StartNew() => new(time.GetUtcNow());

    /// <inheritdoc />
    public WorkoutErrors Validate(IReadOnlyList<WorkoutExercise> exercises) => throw new NotSupportedException();

    /// <inheritdoc />
    public ExerciseErrors ValidateSet(int exerciseId, StrengthSet set) => validator.Validate(new StrengthExercise(exerciseId, [set]));

    /// <inheritdoc />
    public ExerciseErrors ValidateEntry(int exerciseId, CardioEntry entry) => throw new NotSupportedException();

    /// <inheritdoc />
    public Task<SaveResult> SaveNewAsync(WorkoutDraft draft, IReadOnlyList<WorkoutExercise> exercises) => throw new NotSupportedException();

    /// <inheritdoc />
    public Task<SaveResult> SaveAsync(int workoutId, IReadOnlyList<WorkoutExercise> exercises) => throw new NotSupportedException();
}
