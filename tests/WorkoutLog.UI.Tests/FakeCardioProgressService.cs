using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Tests;

/// <summary>
/// <see cref="ICardioProgressService"/> for the tests: loads return <see cref="Result"/>, no sessions by default, and
/// <see cref="Summarise"/> returns <see cref="Summary"/>'s progress. With <see cref="Hold"/> set, each load waits until the test calls <see cref="Complete"/>.
/// </summary>
public sealed class FakeCardioProgressService : ICardioProgressService
{
    private readonly List<TaskCompletionSource<CardioProgressResult>> pending = [];

    /// <summary>What each load returns.</summary>
    public CardioProgressResult Result { get; set; } = new([], Failed: false);

    /// <summary>What each summary returns for the sessions and range; no sessions or records by default.</summary>
    public Func<IReadOnlyList<CardioSession>, ChartRange, CardioProgress> Summary { get; set; } =
        (_, _) => new CardioProgress([], new CardioRecords(null, null, null, null, null, null));

    /// <summary>When true, loads wait for <see cref="Complete"/>.</summary>
    public bool Hold { get; set; }

    /// <summary>Exercise id of each load, in call order.</summary>
    public List<int> Loads { get; } = [];

    /// <summary>Sessions and range of each summary, in call order.</summary>
    public List<(IReadOnlyList<CardioSession> Sessions, ChartRange Range)> Summaries { get; } = [];

    /// <inheritdoc />
    public Task<CardioProgressResult> LoadAsync(int exerciseId)
    {
        Loads.Add(exerciseId);
        if (!Hold)
        {
            return Task.FromResult(Result);
        }

        var load = new TaskCompletionSource<CardioProgressResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        pending.Add(load);
        return load.Task;
    }

    /// <inheritdoc />
    public CardioProgress Summarise(IReadOnlyList<CardioSession> sessions, ChartRange range)
    {
        Summaries.Add((sessions, range));
        return Summary(sessions, range);
    }

    /// <summary>Completes the latest held load with <see cref="Result"/>.</summary>
    public void Complete() => pending[^1].SetResult(Result);
}
