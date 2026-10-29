using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>
/// Reads a cardio exercise's sessions through <see cref="IWorkoutRepository"/> and logs the outcome. Summarises them with the range filter,
/// session and records rules.
/// </summary>
internal sealed partial class CardioProgressService(
    IWorkoutRepository repository,
    IExerciseCatalog catalog,
    IRangeFilter rangeFilter,
    ICardioSessionRule sessionRule,
    ICardioRecordsRule recordsRule,
    ILogger<CardioProgressService> logger) : ICardioProgressService
{
    /// <inheritdoc />
    public async Task<CardioProgressResult> LoadAsync(int exerciseId)
    {
        var definition = catalog.All.FirstOrDefault(e => e.Id == exerciseId);
        if (definition is null)
        {
            Log.UnknownExercise(logger, exerciseId);
            return new CardioProgressResult([], Failed: true);
        }

        if (definition.Category != ExerciseCategory.Cardio)
        {
            Log.NotCardio(logger, exerciseId);
            return new CardioProgressResult([], Failed: true);
        }

        IReadOnlyList<WorkoutSession> sessions;
        try
        {
            sessions = await repository.GetSessionsAsync(exerciseId);
        }
        catch (Exception exception)
        {
            Log.LoadFailed(logger, exerciseId, exception);
            return new CardioProgressResult([], Failed: true);
        }

        var cardio = sessions
            .Where(s => s.Exercise is CardioExercise)
            .Select(s => new CardioSession(s.WorkoutId, s.StartedAt, ((CardioExercise)s.Exercise).Entry))
            .ToList();
        Log.Loaded(logger, cardio.Count, exerciseId);
        return new CardioProgressResult(cardio, Failed: false);
    }

    /// <inheritdoc />
    public CardioProgress Summarise(IReadOnlyList<CardioSession> sessions, ChartRange range)
    {
        var inRange = sessions
            .Where(s => rangeFilter.Includes(s.StartedAt, range))
            .OrderBy(s => s.StartedAt)
            .Select(s => new CardioSessionProgress(s, sessionRule.Best(s.Entry)))
            .ToList();
        return new CardioProgress(inRange, recordsRule.Records(inRange));
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.CardioProgressLoaded, LogLevel.Information, "Loaded {Count} sessions of exercise {ExerciseId}")]
        public static partial void Loaded(ILogger logger, int count, int exerciseId);

        [LoggerMessage((int)CoreEvent.CardioProgressLoadFailed, LogLevel.Error, "Loading the sessions of exercise {ExerciseId} failed")]
        public static partial void LoadFailed(ILogger logger, int exerciseId, Exception exception);

        [LoggerMessage((int)CoreEvent.CardioProgressUnknownExercise, LogLevel.Warning, "Exercise {ExerciseId} is not in the exercise list")]
        public static partial void UnknownExercise(ILogger logger, int exerciseId);

        [LoggerMessage((int)CoreEvent.CardioProgressNotCardio, LogLevel.Warning, "Exercise {ExerciseId} is not cardio")]
        public static partial void NotCardio(ILogger logger, int exerciseId);
    }
}
