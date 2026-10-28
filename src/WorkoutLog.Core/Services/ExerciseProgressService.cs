using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>
/// Reads an exercise's sessions through <see cref="IWorkoutRepository"/> and logs the outcome. Summarises them with the range filter
/// and each type's rules.
/// </summary>
internal sealed partial class ExerciseProgressService(
    IWorkoutRepository repository,
    IExerciseCatalog catalog,
    IRangeFilter rangeFilter,
    ITopSetRule topSetRule,
    IE1rmRule e1rmRule,
    IRecordsRule recordsRule,
    IBodyweightRule bodyweightRule,
    IBodyweightRecordsRule bodyweightRecordsRule,
    ILogger<ExerciseProgressService> logger) : IExerciseProgressService
{
    /// <inheritdoc />
    public async Task<ExerciseProgressResult> LoadAsync(int exerciseId)
    {
        var definition = catalog.All.FirstOrDefault(e => e.Id == exerciseId);
        if (definition is null)
        {
            Log.UnknownExercise(logger, exerciseId);
            return new ExerciseProgressResult([], Failed: true);
        }

        if (!definition.HasProgressPage)
        {
            Log.NotSupported(logger, exerciseId);
            return new ExerciseProgressResult([], Failed: true);
        }

        IReadOnlyList<WorkoutSession> sessions;
        try
        {
            sessions = await repository.GetSessionsAsync(exerciseId);
        }
        catch (Exception exception)
        {
            Log.LoadFailed(logger, exerciseId, exception);
            return new ExerciseProgressResult([], Failed: true);
        }

        var strength = sessions
            .Select(s => new StrengthSession(s.WorkoutId, s.StartedAt, s.Exercises.OfType<StrengthExercise>().SelectMany(e => e.Sets).ToList()))
            .ToList();
        Log.Loaded(logger, strength.Count, exerciseId);
        return new ExerciseProgressResult(strength, Failed: false);
    }

    /// <inheritdoc />
    public LiftProgress SummariseLift(IReadOnlyList<StrengthSession> sessions, ChartRange range)
    {
        var inRange = InRange(sessions, range)
            .Select(s => new LiftSessionProgress(s, topSetRule.TopSet(s.Sets), e1rmRule.Best(s.Sets)))
            .ToList();
        return new LiftProgress(inRange, recordsRule.Records(inRange));
    }

    /// <inheritdoc />
    public BodyweightProgress SummariseBodyweight(IReadOnlyList<StrengthSession> sessions, ChartRange range)
    {
        var inRange = InRange(sessions, range).Select(bodyweightRule.Progress).ToList();
        return new BodyweightProgress(inRange, bodyweightRecordsRule.Records(inRange));
    }

    /// <summary>The sessions in <paramref name="range"/>, by start time oldest first.</summary>
    private IEnumerable<StrengthSession> InRange(IReadOnlyList<StrengthSession> sessions, ChartRange range) =>
        sessions.Where(s => rangeFilter.Includes(s.StartedAt, range)).OrderBy(s => s.StartedAt);

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.ExerciseProgressLoaded, LogLevel.Information, "Loaded {Count} sessions of exercise {ExerciseId}")]
        public static partial void Loaded(ILogger logger, int count, int exerciseId);

        [LoggerMessage((int)CoreEvent.ExerciseProgressLoadFailed, LogLevel.Error, "Loading the sessions of exercise {ExerciseId} failed")]
        public static partial void LoadFailed(ILogger logger, int exerciseId, Exception exception);

        [LoggerMessage((int)CoreEvent.ExerciseProgressUnknownExercise, LogLevel.Warning, "Exercise {ExerciseId} is not in the exercise list")]
        public static partial void UnknownExercise(ILogger logger, int exerciseId);

        [LoggerMessage((int)CoreEvent.ExerciseProgressNotSupported, LogLevel.Warning, "Exercise {ExerciseId} has no progress page")]
        public static partial void NotSupported(ILogger logger, int exerciseId);
    }
}
