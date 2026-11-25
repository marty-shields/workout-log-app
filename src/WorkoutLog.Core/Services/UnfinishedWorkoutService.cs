using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>Reads and writes the unfinished workout through <see cref="IUnfinishedWorkoutRepository"/>. Logs counts, never contents.</summary>
internal sealed partial class UnfinishedWorkoutService(IUnfinishedWorkoutRepository repository, IExerciseCatalog catalog, ILogger<UnfinishedWorkoutService> logger)
    : IUnfinishedWorkoutService
{
    /// <inheritdoc />
    public async Task<UnfinishedSummaryResult> GetSummaryAsync()
    {
        return await ReadAsync() switch
        {
            UnfinishedWorkout workout => UnfinishedSummary.Of(workout, catalog),
            NoUnfinishedWorkout none => none,
            ReadFailed failed => failed,
        };
    }

    /// <inheritdoc />
    public async Task<UnfinishedWorkoutResult> GetAsync()
    {
        var read = await ReadAsync();
        if (read is UnfinishedWorkout workout)
        {
            Log.Recovered(logger, workout.Exercises.Count, UnfinishedSummary.Of(workout, catalog).Sets);
        }

        return read;
    }

    /// <inheritdoc />
    public async Task<UnfinishedWrite> SaveAsync(UnfinishedWorkout workout)
    {
        ArgumentNullException.ThrowIfNull(workout);
        try
        {
            await repository.ReplaceAsync(workout);
        }
        catch (Exception exception)
        {
            Log.SaveFailed(logger, workout.Exercises.Count, exception);
            return UnfinishedWrite.Failed;
        }

        Log.Saved(logger, workout.Exercises.Count, UnfinishedSummary.Of(workout, catalog).Sets);
        return UnfinishedWrite.Done;
    }

    /// <inheritdoc />
    public async Task<UnfinishedWrite> DiscardAsync()
    {
        try
        {
            await repository.DeleteAsync();
        }
        catch (Exception exception)
        {
            Log.DiscardFailed(logger, exception);
            return UnfinishedWrite.Failed;
        }

        Log.Discarded(logger);
        return UnfinishedWrite.Done;
    }

    /// <summary>The workout without the exercises that aren't in the list, <see cref="NoUnfinishedWorkout"/>, or <see cref="ReadFailed"/>.</summary>
    private async Task<UnfinishedWorkoutResult> ReadAsync()
    {
        UnfinishedWorkout? stored;
        try
        {
            stored = await repository.GetAsync();
        }
        catch (Exception exception)
        {
            Log.ReadFailed(logger, exception);
            return new ReadFailed();
        }

        if (stored is null)
        {
            return new NoUnfinishedWorkout();
        }

        var known = stored.Exercises.Where(e => catalog.All.Any(d => d.Id == e.ExerciseId)).ToList();
        foreach (var unknown in stored.Exercises.Except(known))
        {
            Log.UnknownExercise(logger, unknown.ExerciseId);
        }

        return known.Count == 0 ? new NoUnfinishedWorkout() : stored with { Exercises = known };
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.UnfinishedSaved, LogLevel.Debug, "Unfinished workout stored with {ExerciseCount} exercises and {SetCount} sets")]
        public static partial void Saved(ILogger logger, int exerciseCount, int setCount);

        [LoggerMessage((int)CoreEvent.UnfinishedSaveFailed, LogLevel.Error, "Storing the unfinished workout with {ExerciseCount} exercises failed")]
        public static partial void SaveFailed(ILogger logger, int exerciseCount, Exception exception);

        [LoggerMessage((int)CoreEvent.UnfinishedRecovered, LogLevel.Information, "Unfinished workout recovered with {ExerciseCount} exercises and {SetCount} sets")]
        public static partial void Recovered(ILogger logger, int exerciseCount, int setCount);

        [LoggerMessage((int)CoreEvent.UnfinishedReadFailed, LogLevel.Error, "Reading the unfinished workout failed")]
        public static partial void ReadFailed(ILogger logger, Exception exception);

        [LoggerMessage((int)CoreEvent.UnfinishedDiscarded, LogLevel.Information, "Unfinished workout discarded")]
        public static partial void Discarded(ILogger logger);

        [LoggerMessage((int)CoreEvent.UnfinishedDiscardFailed, LogLevel.Error, "Discarding the unfinished workout failed")]
        public static partial void DiscardFailed(ILogger logger, Exception exception);

        [LoggerMessage((int)CoreEvent.UnfinishedUnknownExercise, LogLevel.Warning, "The unfinished workout has exercise {ExerciseId}, which is not in the exercise list")]
        public static partial void UnknownExercise(ILogger logger, int exerciseId);
    }
}
