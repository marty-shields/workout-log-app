using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Tests;

public sealed class FakeWorkoutTotalsService : IWorkoutTotalsService
{
    private readonly List<TaskCompletionSource<ReadResult<WorkoutTotals>>> pending = [];

    public ReadResult<WorkoutTotals> Result { get; set; } = new ReadResult<WorkoutTotals>(new WorkoutTotals(2, 24, TimeSpan.FromMinutes(110), 0.5));

    /// <summary>When true, reads wait for <see cref="Complete"/>.</summary>
    public bool Hold { get; set; }

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

    /// <summary>Completes the held read <paramref name="index"/> from the end; 0 is the latest.</summary>
    public void Complete(ReadResult<WorkoutTotals> result, int index = 0) => pending[^(index + 1)].SetResult(result);
}
