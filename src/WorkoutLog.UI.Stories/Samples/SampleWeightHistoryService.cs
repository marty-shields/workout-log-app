using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary><see cref="IWeightHistoryService"/> for the stories: <see cref="SampleData.Weights"/> for the sample exercises, none for the others.</summary>
public sealed class SampleWeightHistoryService : IWeightHistoryService
{
    /// <inheritdoc />
    public Task<WeightHistoryResult> GetAsync(int exerciseId) =>
        Task.FromResult(SampleData.Weights.TryGetValue(exerciseId, out var weights)
            ? new WeightHistoryResult(weights.LastKg, weights.BestKg, Failed: false)
            : new WeightHistoryResult(null, null, Failed: false));
}
