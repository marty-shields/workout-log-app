using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Tests;

/// <summary>
/// <see cref="IWorkoutTotalsService"/> for the tests: returns <see cref="Result"/>. With <see cref="Hold"/> set, each load waits
/// until the test completes it.
/// </summary>
public sealed class FakeWorkoutTotalsService : IWorkoutTotalsService
{
    private readonly List<TaskCompletionSource<ReadResult<WorkoutTotals>>> pending = [];

    /// <summary>What each load returns.</summary>
    public ReadResult<WorkoutTotals> Result { get; set; } = new ReadResult<WorkoutTotals>(new WorkoutTotals(3, 30, TimeSpan.FromMinutes(150), 2.5));

    /// <summary>When true, loads wait for <see cref="Complete"/>.</summary>
    public bool Hold { get; set; }

    /// <summary>The days of each load, in order.</summary>
    public List<(DateOnly From, DateOnly To)> Reads { get; } = [];

    /// <inheritdoc />
    public Task<ReadResult<WorkoutTotals>> GetAsync(DateOnly from, DateOnly to)
    {
        Reads.Add((from, to));
        if (!Hold)
        {
            return Task.FromResult(Result);
        }

        var load = new TaskCompletionSource<ReadResult<WorkoutTotals>>(TaskCreationOptions.RunContinuationsAsynchronously);
        pending.Add(load);
        return load.Task;
    }

    /// <summary>Completes a held load: the latest, or the one <paramref name="index"/> from the end.</summary>
    public void Complete(ReadResult<WorkoutTotals> result, int index = 0) => pending[^(index + 1)].SetResult(result);
}
