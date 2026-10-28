using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>Reads the saved workouts through <see cref="IWorkoutRepository"/> and logs the outcome.</summary>
internal sealed partial class WorkoutsService(IWorkoutRepository repository, ILogger<WorkoutsService> logger) : IWorkoutsService
{
    /// <inheritdoc />
    public async Task<WorkoutsResult> GetAllAsync()
    {
        IReadOnlyList<WorkoutSummary> workouts;
        try
        {
            workouts = await repository.ListAsync();
        }
        catch (Exception exception)
        {
            Log.LoadFailed(logger, exception);
            return new WorkoutsResult([], Failed: true);
        }

        Log.Loaded(logger, workouts.Count);
        return new WorkoutsResult(workouts, Failed: false);
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.WorkoutsLoaded, LogLevel.Debug, "Loaded {Count} workouts")]
        public static partial void Loaded(ILogger logger, int count);

        [LoggerMessage((int)CoreEvent.WorkoutsLoadFailed, LogLevel.Error, "Loading the workouts failed")]
        public static partial void LoadFailed(ILogger logger, Exception exception);
    }
}
