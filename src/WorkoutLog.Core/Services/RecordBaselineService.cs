using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>Reads an exercise's saved sessions through <see cref="IWorkoutRepository"/> and logs a failure, with no workout contents.</summary>
internal sealed partial class RecordBaselineService(
    IWorkoutRepository repository,
    IExerciseCatalog catalog,
    IEnumerable<IBestRule> rules,
    IRecordCheck recordCheck,
    ILogger<RecordBaselineService> logger) : IRecordBaselineService
{
    /// <inheritdoc />
    public async Task<RecordBaselineResult> LoadAsync(int exerciseId)
    {
        try
        {
            var exercise = catalog.All.Single(e => e.Id == exerciseId);
            var sessions = await repository.GetSessionsAsync(exerciseId);
            return new RecordBaselineResult(new RecordBaseline(exercise, sessions, rules.Single(r => r.AppliesTo(exercise)), recordCheck), Failed: false);
        }
        catch (Exception exception)
        {
            Log.LoadFailed(logger, exerciseId, exception);
            return new RecordBaselineResult(null, Failed: true);
        }
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.RecordBaselineLoadFailed, LogLevel.Warning, "Loading the record baseline of exercise {ExerciseId} failed")]
        public static partial void LoadFailed(ILogger logger, int exerciseId, Exception exception);
    }
}
