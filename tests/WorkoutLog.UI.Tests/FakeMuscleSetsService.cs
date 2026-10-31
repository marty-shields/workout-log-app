using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Tests;

/// <summary>
/// <see cref="IMuscleSetsService"/> for the tests: returns <see cref="Result"/>, no groups by default. With <see cref="Hold"/> set,
/// each load waits until the test calls <see cref="Complete"/>.
/// </summary>
public sealed class FakeMuscleSetsService : IMuscleSetsService
{
    private readonly List<TaskCompletionSource<MuscleSetsResult>> pending = [];

    /// <summary>What each load returns.</summary>
    public MuscleSetsResult Result { get; set; } = new([], Failed: false);

    /// <summary>When true, loads wait for <see cref="Complete"/>.</summary>
    public bool Hold { get; set; }

    public int Loads { get; private set; }

    /// <inheritdoc />
    public Task<MuscleSetsResult> GetAsync()
    {
        Loads++;
        if (!Hold)
        {
            return Task.FromResult(Result);
        }

        var load = new TaskCompletionSource<MuscleSetsResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        pending.Add(load);
        return load.Task;
    }

    /// <summary>Completes the latest held load with <see cref="Result"/>.</summary>
    public void Complete() => pending[^1].SetResult(Result);
}
