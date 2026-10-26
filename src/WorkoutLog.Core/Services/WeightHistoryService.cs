using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>
/// Reads an exercise's weights through <see cref="IWorkoutRepository"/> and logs a failure. An exercise whose weight is
/// labelled <see cref="AssistLabel"/> takes the lowest as best (D237).
/// </summary>
internal sealed partial class WeightHistoryService(IWorkoutRepository repository, IExerciseCatalog catalog, ILogger<WeightHistoryService> logger)
    : IWeightHistoryService
{
    /// <summary>Weight label of the assisted machines.</summary>
    private const string AssistLabel = "Assist";

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

        Func<IEnumerable<decimal>, decimal?> best = IsAssisted(exerciseId)
            ? weights => weights.Select(w => (decimal?)w).Min()
            : weights => weights.Select(w => (decimal?)w).Max();
        return new WeightHistoryResult(
            workouts.Count == 0 ? null : best(workouts[0].WeightsKg),
            best(workouts.SelectMany(w => w.WeightsKg)),
            Failed: false);
    }

    private bool IsAssisted(int exerciseId) =>
        catalog.All.FirstOrDefault(e => e.Id == exerciseId) is { } exercise
        && exercise.Metrics.Any(m => m is { Metric: Metric.WeightKg, Label: AssistLabel });

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.WeightHistoryLoadFailed, LogLevel.Error, "Loading the weight history of exercise {ExerciseId} failed")]
        public static partial void LoadFailed(ILogger logger, int exerciseId, Exception exception);
    }
}
