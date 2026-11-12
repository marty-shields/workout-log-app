using WorkoutLog.Core.Services;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests;

/// <summary><see cref="IWeightHistoryService"/> for the tests: <see cref="SampleData.Weights"/> for the sample exercises, none for the others.</summary>
public sealed class SampleWeightHistoryService : IWeightHistoryService
{
    /// <inheritdoc />
    public Task<ReadResult<ExerciseHistory>> GetAsync(int exerciseId, DateTimeOffset? before) =>
        Task.FromResult(SampleData.Weights.TryGetValue(exerciseId, out var weights)
            ? new ReadResult<ExerciseHistory>(new ExerciseHistory(weights.LastKg, weights.BestKg, null))
            : new ReadResult<ExerciseHistory>(new ExerciseHistory(null, null, null)));
}
