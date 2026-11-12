using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Tests;

/// <summary><see cref="IWeightHistoryService"/> for the tests: records each read and returns the exercise's <see cref="Results"/> entry, or no history.</summary>
public sealed class FakeWeightHistoryService : IWeightHistoryService
{
    /// <summary>The result per exercise id; an exercise with none has no history.</summary>
    public Dictionary<int, ReadResult<ExerciseHistory>> Results { get; } = [];

    /// <summary>The exercise id and <c>before</c> of each read, in call order.</summary>
    public List<(int ExerciseId, DateTimeOffset? Before)> Calls { get; } = [];

    /// <inheritdoc />
    public Task<ReadResult<ExerciseHistory>> GetAsync(int exerciseId, DateTimeOffset? before)
    {
        Calls.Add((exerciseId, before));
        return Task.FromResult(Results.GetValueOrDefault(exerciseId, new ReadResult<ExerciseHistory>(new ExerciseHistory(null, null, null))));
    }
}
