using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;

namespace WorkoutLog.Core.Services;

/// <summary>Checks workouts with <see cref="IWorkoutValidator"/> and saves them through <see cref="IWorkoutRepository"/>.</summary>
internal sealed partial class WorkoutService(
    IWorkoutRepository repository, IWorkoutValidator validator, TimeProvider time, ILogger<WorkoutService> logger) : IWorkoutService
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
        if (now < draft.StartedAt)
        {
            Log.ClockBeforeStart(logger, draft.StartedAt - now);
        }

        var workout = new NewWorkout(draft.StartedAt, now < draft.StartedAt ? draft.StartedAt : now, copy);
        return await WriteAsync(null, copy.Length, () => repository.AddAsync(workout), errors);
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

        return await WriteAsync(workoutId, copy.Length, async () =>
        {
            await repository.ReplaceExercisesAsync(workoutId, copy);
            return workoutId;
        }, errors);
    }

    /// <summary>Runs <paramref name="write"/> and logs the outcome. <paramref name="workoutId"/> is null for a new workout.</summary>
    private async Task<SaveResult> WriteAsync(int? workoutId, int exerciseCount, Func<Task<int>> write, WorkoutErrors errors)
    {
        int savedId;
        try
        {
            savedId = await write();
        }
        catch (Exception exception)
        {
            if (workoutId is { } id)
            {
                Log.UpdateFailed(logger, id, exception);
            }
            else
            {
                Log.AddFailed(logger, exception);
            }

            return new SaveResult(null, SaveFailed);
        }

        if (workoutId is null)
        {
            Log.Added(logger, savedId, exerciseCount);
        }
        else
        {
            Log.Updated(logger, savedId, exerciseCount);
        }

        return new SaveResult(savedId, errors);
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

    private static partial class Log
    {
        [LoggerMessage(1201, LogLevel.Information, "Workout {WorkoutId} added with {ExerciseCount} exercises")]
        public static partial void Added(ILogger logger, int workoutId, int exerciseCount);

        [LoggerMessage(1202, LogLevel.Information, "Workout {WorkoutId} updated with {ExerciseCount} exercises")]
        public static partial void Updated(ILogger logger, int workoutId, int exerciseCount);

        [LoggerMessage(1203, LogLevel.Error, "Adding a workout failed")]
        public static partial void AddFailed(ILogger logger, Exception exception);

        [LoggerMessage(1204, LogLevel.Error, "Updating workout {WorkoutId} failed")]
        public static partial void UpdateFailed(ILogger logger, int workoutId, Exception exception);

        [LoggerMessage(1205, LogLevel.Warning, "The clock is {Behind} behind the workout start; the finish is set to the start")]
        public static partial void ClockBeforeStart(ILogger logger, TimeSpan behind);
    }
}
