using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;

namespace WorkoutLog.Core.Services;

/// <summary>Checks workouts with <see cref="IWorkoutValidator"/> and saves them through <see cref="IWorkoutRepository"/>.</summary>
internal sealed class WorkoutService(IWorkoutRepository repository, IWorkoutValidator validator, TimeProvider time) : IWorkoutService
{
    private static readonly WorkoutErrors SaveFailed = new(WorkoutError.SaveFailed, []);

    /// <inheritdoc />
    public WorkoutDraft StartNew() => new(time.GetUtcNow());

    /// <inheritdoc />
    public WorkoutErrors Validate(IReadOnlyList<WorkoutExercise> exercises) => validator.Validate(Snapshot(exercises));

    /// <inheritdoc />
    public ExerciseErrors ValidateSet(int exerciseId, StrengthSet set)
    {
        ArgumentNullException.ThrowIfNull(set);
        return validator.Validate(new StrengthExercise(exerciseId, [set]));
    }

    /// <inheritdoc />
    public ExerciseErrors ValidateEntry(int exerciseId, CardioEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return validator.Validate(new CardioExercise(exerciseId, entry));
    }

    /// <inheritdoc />
    public async Task<SaveResult> SaveNewAsync(WorkoutDraft draft, IReadOnlyList<WorkoutExercise> exercises)
    {
        ArgumentNullException.ThrowIfNull(draft);
        var copy = Snapshot(exercises);
        var errors = validator.Validate(copy);
        if (!errors.IsValid)
        {
            return new SaveResult(null, errors);
        }

        var now = time.GetUtcNow();
        var workout = new NewWorkout(draft.StartedAt, now < draft.StartedAt ? draft.StartedAt : now, copy);
        return await WriteAsync(() => repository.AddAsync(workout), errors);
    }

    /// <inheritdoc />
    public async Task<SaveResult> SaveAsync(int workoutId, IReadOnlyList<WorkoutExercise> exercises)
    {
        var copy = Snapshot(exercises);
        var errors = validator.Validate(copy);
        if (!errors.IsValid)
        {
            return new SaveResult(null, errors);
        }

        return await WriteAsync(async () =>
        {
            await repository.ReplaceExercisesAsync(workoutId, copy);
            return workoutId;
        }, errors);
    }

    private static async Task<SaveResult> WriteAsync(Func<Task<int>> write, WorkoutErrors errors)
    {
        try
        {
            return new SaveResult(await write(), errors);
        }
        catch (Exception)
        {
            return new SaveResult(null, SaveFailed);
        }
    }

    /// <summary>Copies the list and its sets, so later changes by the caller aren't saved. Throws on a null list, exercise, set list, set or entry.</summary>
    private static WorkoutExercise[] Snapshot(IReadOnlyList<WorkoutExercise> exercises)
    {
        ArgumentNullException.ThrowIfNull(exercises);
        return exercises.Select(exercise => exercise switch
        {
            null => throw new ArgumentNullException(nameof(exercises), "An exercise is null"),
            StrengthExercise { Sets: null } => throw new ArgumentNullException(nameof(exercises), "An exercise's sets are null"),
            StrengthExercise strength when strength.Sets.Contains(null!) => throw new ArgumentNullException(nameof(exercises), "A set is null"),
            StrengthExercise strength => strength with { Sets = strength.Sets.ToArray() },
            CardioExercise { Entry: null } => throw new ArgumentNullException(nameof(exercises), "A cardio entry is null"),
            _ => exercise,
        }).ToArray();
    }
}
