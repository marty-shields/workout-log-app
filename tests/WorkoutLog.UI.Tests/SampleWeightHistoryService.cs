using WorkoutLog.Core.Services;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests;

/// <summary><see cref="IWeightHistoryService"/> for the tests: <see cref="SampleData.Weights"/> for the sample exercises, none for the others.</summary>
public sealed class SampleWeightHistoryService : IWeightHistoryService
{
    /// <inheritdoc />
    public Task<WeightHistoryResult> GetAsync(int exerciseId) =>
        Task.FromResult(SampleData.Weights.TryGetValue(exerciseId, out var weights)
            ? new WeightHistoryResult(weights.LastKg, weights.BestKg, Failed: false)
            : new WeightHistoryResult(null, null, Failed: false));
}
