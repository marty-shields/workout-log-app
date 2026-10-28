using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Tests;

/// <summary>
/// <see cref="IExerciseProgressService"/> for the tests: loads return <see cref="Result"/>, no sessions by default, and
/// <see cref="SummariseLift"/> and <see cref="SummariseBodyweight"/> return <see cref="Summary"/>'s and <see cref="BodyweightSummary"/>'s progress.
/// With <see cref="Hold"/> set, each load waits until the test calls <see cref="Complete"/>.
/// </summary>
public sealed class FakeExerciseProgressService : IExerciseProgressService
{
    private readonly List<TaskCompletionSource<ExerciseProgressResult>> pending = [];

    /// <summary>What each load returns.</summary>
    public ExerciseProgressResult Result { get; set; } = new([], Failed: false);

    /// <summary>What each lift summary returns for the sessions and range; no sessions or records by default.</summary>
    public Func<IReadOnlyList<StrengthSession>, ChartRange, LiftProgress> Summary { get; set; } = (_, _) => new LiftProgress([], new LiftRecords(null, null));

    /// <summary>What each bodyweight summary returns for the sessions and range; no sessions or records by default.</summary>
    public Func<IReadOnlyList<StrengthSession>, ChartRange, BodyweightProgress> BodyweightSummary { get; set; } =
        (_, _) => new BodyweightProgress([], new BodyweightRecords(null, null, null, null));

    /// <summary>When true, loads wait for <see cref="Complete"/>.</summary>
    public bool Hold { get; set; }

    /// <summary>Exercise id of each load, in call order.</summary>
    public List<int> Loads { get; } = [];

    /// <summary>Sessions and range of each lift summary, in call order.</summary>
    public List<(IReadOnlyList<StrengthSession> Sessions, ChartRange Range)> LiftSummaries { get; } = [];

    /// <summary>Sessions and range of each bodyweight summary, in call order.</summary>
    public List<(IReadOnlyList<StrengthSession> Sessions, ChartRange Range)> BodyweightSummaries { get; } = [];

    /// <inheritdoc />
    public Task<ExerciseProgressResult> LoadAsync(int exerciseId)
    {
        Loads.Add(exerciseId);
        if (!Hold)
        {
            return Task.FromResult(Result);
        }

        var load = new TaskCompletionSource<ExerciseProgressResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        pending.Add(load);
        return load.Task;
    }

    /// <inheritdoc />
    public LiftProgress SummariseLift(IReadOnlyList<StrengthSession> sessions, ChartRange range)
    {
        LiftSummaries.Add((sessions, range));
        return Summary(sessions, range);
    }

    /// <inheritdoc />
    public BodyweightProgress SummariseBodyweight(IReadOnlyList<StrengthSession> sessions, ChartRange range)
    {
        BodyweightSummaries.Add((sessions, range));
        return BodyweightSummary(sessions, range);
    }

    /// <summary>Completes the latest held load with <see cref="Result"/>.</summary>
    public void Complete() => pending[^1].SetResult(Result);
}
