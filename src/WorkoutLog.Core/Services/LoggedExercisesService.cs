using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>
/// Reads each exercise's last session through <see cref="IWorkoutRepository"/>, picks its best with the exercise's <see cref="IBestRule"/>
/// and logs the outcome.
/// </summary>
internal sealed partial class LoggedExercisesService(
    IWorkoutRepository repository,
    IExerciseCatalog catalog,
    IEnumerable<IBestRule> rules,
    ILogger<LoggedExercisesService> logger) : ILoggedExercisesService
{
    private readonly IReadOnlyList<IBestRule> rules = rules.ToList();

    /// <inheritdoc />
    public async Task<LoggedExercisesResult> GetAsync()
    {
        IReadOnlyList<LastSession> sessions;
        try
        {
            sessions = await repository.GetLastSessionsAsync();
        }
        catch (Exception exception)
        {
            Log.LoadFailed(logger, exception);
            return new LoggedExercisesResult([], Failed: true);
        }

        var definitions = catalog.All.ToDictionary(e => e.Id);
        var exercises = new List<(LoggedExercise Exercise, string Name)>();
        foreach (var session in sessions)
        {
            if (!definitions.TryGetValue(session.ExerciseId, out var definition))
            {
                Log.UnknownExercise(logger, session.ExerciseId);
                continue;
            }

            var best = rules.FirstOrDefault(r => r.AppliesTo(definition))?.Best(definition, session.Exercises);
            exercises.Add((new LoggedExercise(session.ExerciseId, session.StartedAt, best), definition.Name));
        }

        var ordered = exercises
            .OrderByDescending(e => e.Exercise.LastStartedAt)
            .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .Select(e => e.Exercise)
            .ToList();
        Log.Loaded(logger, ordered.Count);
        return new LoggedExercisesResult(ordered, Failed: false);
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.LoggedExercisesLoaded, LogLevel.Information, "Loaded {Count} logged exercises")]
        public static partial void Loaded(ILogger logger, int count);

        [LoggerMessage((int)CoreEvent.LoggedExercisesLoadFailed, LogLevel.Warning, "Loading the logged exercises failed")]
        public static partial void LoadFailed(ILogger logger, Exception exception);

        [LoggerMessage((int)CoreEvent.LoggedExercisesUnknownExercise, LogLevel.Warning, "Exercise {ExerciseId} is not in the exercise list; left out")]
        public static partial void UnknownExercise(ILogger logger, int exerciseId);
    }
}
