using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Tests;

/// <summary>
/// <see cref="ILoggedExercisesService"/> for the tests: returns <see cref="Result"/>, nothing logged by default. With
/// <see cref="Hold"/> set, each load waits until the test calls <see cref="Complete"/>.
/// </summary>
public sealed class FakeLoggedExercisesService : ILoggedExercisesService
{
    private readonly List<TaskCompletionSource<LoggedExercisesResult>> pending = [];

    /// <summary>What each load returns.</summary>
    public LoggedExercisesResult Result { get; set; } = new([], Failed: false);

    /// <summary>When true, loads wait for <see cref="Complete"/>.</summary>
    public bool Hold { get; set; }

    public int Loads { get; private set; }

    /// <inheritdoc />
    public Task<LoggedExercisesResult> GetAsync()
    {
        Loads++;
        if (!Hold)
        {
            return Task.FromResult(Result);
        }

        var load = new TaskCompletionSource<LoggedExercisesResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        pending.Add(load);
        return load.Task;
    }

    /// <summary>Completes a held load with <see cref="Result"/>: the latest, or the one at <paramref name="index"/> in call order.</summary>
    public void Complete(int? index = null) => pending[index ?? pending.Count - 1].SetResult(Result);
}
