using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>
/// Reads an exercise's weights through <see cref="IWorkoutRepository"/> and logs a failure. An assisted machine
/// (<see cref="ExerciseDefinition.IsAssisted"/>) takes the lowest as best (D48).
/// </summary>
internal sealed partial class WeightHistoryService(IWorkoutRepository repository, IExerciseCatalog catalog, ILogger<WeightHistoryService> logger)
    : IWeightHistoryService
{
    /// <inheritdoc />
    public async Task<WeightHistoryResult> GetAsync(int exerciseId)
    {
        IReadOnlyList<WorkoutWeights> workouts;
        try
        {
            workouts = await repository.GetWeightsAsync(exerciseId);
        }
        catch (Exception exception)
        {
            Log.LoadFailed(logger, exerciseId, exception);
            return new WeightHistoryResult(null, null, Failed: true);
        }

        Func<IEnumerable<decimal>, decimal?> best = catalog.IsAssisted(exerciseId)
            ? weights => weights.Select(w => (decimal?)w).Min()
            : weights => weights.Select(w => (decimal?)w).Max();
        return new WeightHistoryResult(
            workouts.Count == 0 ? null : best(workouts[0].WeightsKg),
            best(workouts.SelectMany(w => w.WeightsKg)),
            Failed: false);
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.WeightHistoryLoadFailed, LogLevel.Error, "Loading the weight history of exercise {ExerciseId} failed")]
        public static partial void LoadFailed(ILogger logger, int exerciseId, Exception exception);
    }
}
