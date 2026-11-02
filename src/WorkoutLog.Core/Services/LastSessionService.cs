using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>
/// Reads an exercise's last session through <see cref="IWorkoutRepository"/> and logs a failure. A cardio entry's chart values
/// come from <see cref="CardioSessionRule"/>.
/// </summary>
internal sealed partial class LastSessionService(IWorkoutRepository repository, CardioSessionRule cardioRule, ILogger<LastSessionService> logger)
    : ILastSessionService
{
    /// <inheritdoc />
    public async Task<LastSessionResult> GetAsync(int exerciseId, DateTimeOffset? before = null)
    {
        LastSession? last;
        try
        {
            last = await repository.GetLastSessionOfAsync(exerciseId, before);
        }
        catch (Exception exception)
        {
            Log.LoadFailed(logger, exerciseId, exception);
            return new LastSessionResult(null, null, Failed: true);
        }

        return last?.Exercise switch
        {
            null => new LastSessionResult(null, null, Failed: false),
            CardioExercise cardio => new LastSessionResult(cardio, cardioRule.Best(cardio.Entry), Failed: false),
            var exercise => new LastSessionResult(exercise, null, Failed: false),
        };
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.LastSessionLoadFailed, LogLevel.Warning, "Loading the last session of exercise {ExerciseId} failed")]
        public static partial void LoadFailed(ILogger logger, int exerciseId, Exception exception);
    }
}
