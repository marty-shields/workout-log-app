using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Tests;

/// <summary>
/// <see cref="IWorkoutSummaryService"/> for the tests: returns <see cref="Result"/>, a failed load by default, and keeps the ids asked for.
/// With <see cref="Hold"/> set, each load waits until the test calls <see cref="Complete"/>.
/// </summary>
public sealed class FakeWorkoutSummaryService : IWorkoutSummaryService
{
    private readonly List<TaskCompletionSource<WorkoutSummaryResult>> pending = [];

    /// <summary>What each load returns.</summary>
    public WorkoutSummaryResult Result { get; set; } = new(null, Failed: true);

    /// <summary>When true, loads wait for <see cref="Complete"/>.</summary>
    public bool Hold { get; set; }

    /// <summary>The workout id of each load.</summary>
    public List<int> Loads { get; } = [];

    /// <inheritdoc />
    public Task<WorkoutSummaryResult> GetAsync(int workoutId)
    {
        Loads.Add(workoutId);
        if (!Hold)
        {
            return Task.FromResult(Result);
        }

        var load = new TaskCompletionSource<WorkoutSummaryResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        pending.Add(load);
        return load.Task;
    }

    /// <summary>Completes the latest held load with <see cref="Result"/>.</summary>
    public void Complete() => pending[^1].SetResult(Result);
}
