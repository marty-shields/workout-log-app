using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Tests;

/// <summary>
/// <see cref="IWorkoutTotalsService"/> for the tests: returns <see cref="Result"/>, no workouts by default. With <see cref="Hold"/> set,
/// each load waits until the test calls <see cref="Complete"/>.
/// </summary>
public sealed class FakeWorkoutTotalsService : IWorkoutTotalsService
{
    private readonly List<TaskCompletionSource<ReadResult<WorkoutTotals>>> pending = [];

    /// <summary>What each load returns.</summary>
    public ReadResult<WorkoutTotals> Result { get; set; } = new ReadResult<WorkoutTotals>(new WorkoutTotals(0, 0, TimeSpan.Zero, 0));

    /// <summary>When true, loads wait for <see cref="Complete"/>.</summary>
    public bool Hold { get; set; }

    /// <summary>The days of each load, in order.</summary>
    public List<(DateOnly From, DateOnly To)> Loads { get; } = [];

    /// <inheritdoc />
    public Task<ReadResult<WorkoutTotals>> GetAsync(DateOnly from, DateOnly to)
    {
        Loads.Add((from, to));
        if (!Hold)
        {
            return Task.FromResult(Result);
        }

        var load = new TaskCompletionSource<ReadResult<WorkoutTotals>>(TaskCreationOptions.RunContinuationsAsynchronously);
        pending.Add(load);
        return load.Task;
    }

    /// <summary>Completes a held load with <paramref name="result"/>: the latest, or the one <paramref name="index"/> from the end.</summary>
    public void Complete(ReadResult<WorkoutTotals> result, int index = 0) => pending[^(index + 1)].SetResult(result);
}
