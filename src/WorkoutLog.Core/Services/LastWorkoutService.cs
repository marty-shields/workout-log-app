using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>Reads the last workout through <see cref="IWorkoutRepository"/>, leaves out exercises not in the list and logs the outcome.</summary>
internal sealed partial class LastWorkoutService(IWorkoutRepository repository, IExerciseCatalog catalog, ILogger<LastWorkoutService> logger)
    : ILastWorkoutService
{
    /// <inheritdoc />
    public async Task<LastWorkoutResult> GetAsync()
    {
        WorkoutOutline? workout;
        try
        {
            workout = await repository.GetLatestAsync();
        }
        catch (Exception exception)
        {
            Log.LoadFailed(logger, exception);
            return new LastWorkoutResult(null, Failed: true);
        }

        if (workout is not null)
        {
            var known = catalog.All.Select(e => e.Id).ToHashSet();
            foreach (var id in workout.ExerciseIds.Where(id => !known.Contains(id)))
            {
                Log.UnknownExercise(logger, id);
            }

            workout = workout with { ExerciseIds = workout.ExerciseIds.Where(known.Contains).ToList() };
        }

        Log.Loaded(logger);
        return new LastWorkoutResult(workout, Failed: false);
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.LastWorkoutLoaded, LogLevel.Information, "Last workout loaded")]
        public static partial void Loaded(ILogger logger);

        [LoggerMessage((int)CoreEvent.LastWorkoutLoadFailed, LogLevel.Warning, "Loading the last workout failed")]
        public static partial void LoadFailed(ILogger logger, Exception exception);

        [LoggerMessage((int)CoreEvent.LastWorkoutUnknownExercise, LogLevel.Warning, "Exercise {ExerciseId} is not in the exercise list; left out")]
        public static partial void UnknownExercise(ILogger logger, int exerciseId);
    }
}
