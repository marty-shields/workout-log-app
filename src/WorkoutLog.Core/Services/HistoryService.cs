using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Services;

/// <summary>Reads the history through <see cref="IWorkoutRepository"/> and logs the outcome.</summary>
internal sealed partial class HistoryService(IWorkoutRepository repository, ILogger<HistoryService> logger) : IHistoryService
{
    /// <inheritdoc />
    public async Task<HistoryResult> GetAllAsync()
    {
        IReadOnlyList<WorkoutSummary> workouts;
        try
        {
            workouts = await repository.ListAsync();
        }
        catch (Exception exception)
        {
            Log.LoadFailed(logger, exception);
            return new HistoryResult([], Failed: true);
        }

        Log.Loaded(logger, workouts.Count);
        return new HistoryResult(workouts, Failed: false);
    }

    private static partial class Log
    {
        [LoggerMessage(1301, LogLevel.Debug, "History loaded with {Count} workouts")]
        public static partial void Loaded(ILogger logger, int count);

        [LoggerMessage(1302, LogLevel.Error, "Loading the history failed")]
        public static partial void LoadFailed(ILogger logger, Exception exception);
    }
}
