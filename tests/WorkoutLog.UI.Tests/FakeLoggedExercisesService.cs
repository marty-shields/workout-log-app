using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Tests;

/// <summary>
/// <see cref="ILoggedExercisesService"/> for the tests: returns <see cref="Result"/>, nothing logged by default. With
/// <see cref="Hold"/> set, each load waits until the test calls <see cref="Complete"/>.
/// </summary>
public sealed class FakeLoggedExercisesService : ILoggedExercisesService
{
    private readonly List<TaskCompletionSource<ReadResult<IReadOnlyList<LoggedExercise>>>> pending = [];

    /// <summary>What each load returns.</summary>
    public ReadResult<IReadOnlyList<LoggedExercise>> Result { get; set; } = new ReadResult<IReadOnlyList<LoggedExercise>>([]);

    /// <summary>When true, loads wait for <see cref="Complete"/>.</summary>
    public bool Hold { get; set; }

    public int Loads { get; private set; }

    /// <inheritdoc />
    public Task<ReadResult<IReadOnlyList<LoggedExercise>>> GetAsync()
    {
        Loads++;
        if (!Hold)
        {
            return Task.FromResult(Result);
        }

        var load = new TaskCompletionSource<ReadResult<IReadOnlyList<LoggedExercise>>>(TaskCreationOptions.RunContinuationsAsynchronously);
        pending.Add(load);
        return load.Task;
    }

    /// <summary>Completes a held load with <see cref="Result"/>: the latest, or the one at <paramref name="index"/> in call order.</summary>
    public void Complete(int? index = null) => pending[index ?? pending.Count - 1].SetResult(Result);
}
