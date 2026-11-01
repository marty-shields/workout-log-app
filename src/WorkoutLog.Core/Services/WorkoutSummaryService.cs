using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>
/// Reads every saved workout through <see cref="IWorkoutRepository"/>, finds the records with <see cref="IRecordCheck"/>, compares each best with
/// the last session before the workout with <see cref="IBestChangeRule"/> and logs the outcome.
/// </summary>
internal sealed partial class WorkoutSummaryService(
    IWorkoutRepository repository,
    IExerciseCatalog catalog,
    IRecordCheck recordCheck,
    IBestChangeRule changeRule,
    ILogger<WorkoutSummaryService> logger) : IWorkoutSummaryService
{
    /// <inheritdoc />
    public async Task<WorkoutSummaryResult> GetAsync(int workoutId)
    {
        try
        {
            var workouts = await repository.GetAllAsync();
            var workout = workouts.SingleOrDefault(w => w.Id == workoutId) ?? throw new KeyNotFoundException($"Workout {workoutId} does not exist.");
            var sessions = workouts
                .Where(w => w.StartedAt <= workout.StartedAt)
                .SelectMany(w => w.Exercises.Select(e => new WorkoutSession(w.Id, w.StartedAt, e)))
                .ToLookup(s => s.Exercise.ExerciseId);
            var definitions = catalog.All.ToDictionary(e => e.Id);
            var exercises = new List<SummaryExercise>();
            foreach (var logged in workout.Exercises)
            {
                if (!definitions.TryGetValue(logged.ExerciseId, out var definition))
                {
                    Log.UnknownExercise(logger, logged.ExerciseId);
                    continue;
                }

                try
                {
                    var history = sessions[logged.ExerciseId].ToList();
                    var earlier = history.Where(s => s.StartedAt < workout.StartedAt).Select(s => s.Exercise);
                    if (changeRule.Change(definition, logged, earlier) is { } session)
                    {
                        var isRecord = recordCheck.Records(definition, history).Any(r => r.Session.WorkoutId == workoutId);
                        exercises.Add(new SummaryExercise(logged.ExerciseId, session.Best, isRecord, session.Change));
                    }
                }
                catch (InvalidOperationException exception)
                {
                    Log.ExerciseFailed(logger, logged.ExerciseId, exception);
                }
            }

            var sets = workout.Exercises.OfType<StrengthExercise>().Sum(e => e.Sets.Count);
            var hasEarlierWorkout = workouts.Any(w => w.StartedAt < workout.StartedAt);
            Log.Loaded(logger, workoutId);
            return new WorkoutSummaryResult(
                new SavedWorkoutSummary(workout.FinishedAt - workout.StartedAt, workout.Exercises.Count, sets, hasEarlierWorkout, exercises),
                Failed: false);
        }
        catch (Exception exception)
        {
            Log.LoadFailed(logger, exception);
            return new WorkoutSummaryResult(null, Failed: true);
        }
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.WorkoutSummaryLoaded, LogLevel.Information, "Loaded the summary of workout {WorkoutId}")]
        public static partial void Loaded(ILogger logger, int workoutId);

        [LoggerMessage((int)CoreEvent.WorkoutSummaryLoadFailed, LogLevel.Warning, "Loading the workout summary failed")]
        public static partial void LoadFailed(ILogger logger, Exception exception);

        [LoggerMessage((int)CoreEvent.WorkoutSummaryUnknownExercise, LogLevel.Warning, "Exercise {ExerciseId} is not in the exercise list; left out")]
        public static partial void UnknownExercise(ILogger logger, int exerciseId);

        [LoggerMessage((int)CoreEvent.WorkoutSummaryExerciseFailed, LogLevel.Warning, "Summarising exercise {ExerciseId} failed; left out")]
        public static partial void ExerciseFailed(ILogger logger, int exerciseId, Exception exception);
    }
}
