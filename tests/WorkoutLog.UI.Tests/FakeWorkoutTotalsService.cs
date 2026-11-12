using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Tests;

/// <summary>
/// <see cref="IWorkoutTotalsService"/> for the tests: returns <see cref="Result"/> and records the days of each read. With <see cref="Hold"/> set,
/// each read waits until the test calls <see cref="Complete"/>.
/// </summary>
public sealed class FakeWorkoutTotalsService : IWorkoutTotalsService
{
    private readonly List<TaskCompletionSource<ReadResult<WorkoutTotals>>> pending = [];

    /// <summary>What each read returns.</summary>
    public ReadResult<WorkoutTotals> Result { get; set; } = new ReadResult<WorkoutTotals>(new WorkoutTotals(2, 24, TimeSpan.FromMinutes(110), 0.5));

    /// <summary>When true, reads wait for <see cref="Complete"/>.</summary>
    public bool Hold { get; set; }

    /// <summary>The days of each read, in order.</summary>
    public List<(DateOnly From, DateOnly To)> Reads { get; } = [];

    /// <inheritdoc />
    public Task<ReadResult<WorkoutTotals>> GetAsync(DateOnly from, DateOnly to)
    {
        Reads.Add((from, to));
        if (!Hold)
        {
            return Task.FromResult(Result);
        }

        var read = new TaskCompletionSource<ReadResult<WorkoutTotals>>(TaskCreationOptions.RunContinuationsAsynchronously);
        pending.Add(read);
        return read.Task;
    }

    /// <summary>Completes a held read with <paramref name="result"/>: the latest, or the one <paramref name="index"/> from the end.</summary>
    public void Complete(ReadResult<WorkoutTotals> result, int index = 0) => pending[^(index + 1)].SetResult(result);
}
