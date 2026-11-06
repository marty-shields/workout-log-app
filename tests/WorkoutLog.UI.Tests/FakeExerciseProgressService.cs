using WorkoutLog.Core.Domain;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Tests;

/// <summary>
/// <see cref="IExerciseProgressService"/> for the tests: loads return <see cref="Result"/>, no sessions by default, and each summarise returns
/// its summary function's progress and records its sessions and range. With <see cref="Hold"/> set, each load waits until the test calls
/// <see cref="Complete"/>.
/// </summary>
public sealed class FakeExerciseProgressService : IExerciseProgressService
{
    private readonly List<TaskCompletionSource<ExerciseProgressResult>> pending = [];

    /// <summary>What each load returns.</summary>
    public ExerciseProgressResult Result { get; set; } = new([], Failed: false);

    /// <summary>What each lift summary returns for the sessions and range; no sessions or records by default.</summary>
    public Func<IReadOnlyList<StrengthSession>, HistoryRange, LiftProgress> Summary { get; set; } = (_, _) => new LiftProgress([], new LiftRecords(null, null));

    /// <summary>What each bodyweight summary returns for the sessions and range; no sessions or records by default.</summary>
    public Func<IReadOnlyList<StrengthSession>, HistoryRange, BodyweightProgress> BodyweightSummary { get; set; } =
        (_, _) => new BodyweightProgress([], new BodyweightRecords(null, null, null, null));

    /// <summary>What each assisted machine summary returns; no sessions or records by default.</summary>
    public Func<IReadOnlyList<StrengthSession>, HistoryRange, AssistedProgress> AssistedSummary { get; set; } =
        (_, _) => new AssistedProgress([], new AssistedRecords(null));

    /// <summary>What each hold summary returns; no sessions or records by default.</summary>
    public Func<IReadOnlyList<StrengthSession>, HistoryRange, HoldProgress> HoldSummary { get; set; } = (_, _) => new HoldProgress([], new HoldRecords(null, null));

    /// <summary>What each carry summary returns; no sessions or records by default.</summary>
    public Func<IReadOnlyList<StrengthSession>, HistoryRange, CarryProgress> CarrySummary { get; set; } = (_, _) => new CarryProgress([], new CarryRecords(null, null));

    /// <summary>When true, loads wait for <see cref="Complete"/>.</summary>
    public bool Hold { get; set; }

    /// <summary>Exercise id of each load, in call order.</summary>
    public List<int> Loads { get; } = [];

    /// <summary>Sessions and range of each lift summary, in call order.</summary>
    public List<(IReadOnlyList<StrengthSession> Sessions, HistoryRange Range)> LiftSummaries { get; } = [];

    /// <summary>Sessions and range of each bodyweight summary, in call order.</summary>
    public List<(IReadOnlyList<StrengthSession> Sessions, HistoryRange Range)> BodyweightSummaries { get; } = [];

    /// <summary>Sessions and range of each assisted machine summary, in call order.</summary>
    public List<(IReadOnlyList<StrengthSession> Sessions, HistoryRange Range)> AssistedSummaries { get; } = [];

    /// <summary>Sessions and range of each hold summary, in call order.</summary>
    public List<(IReadOnlyList<StrengthSession> Sessions, HistoryRange Range)> HoldSummaries { get; } = [];

    /// <summary>Sessions and range of each carry summary, in call order.</summary>
    public List<(IReadOnlyList<StrengthSession> Sessions, HistoryRange Range)> CarrySummaries { get; } = [];

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
    public LiftProgress SummariseLift(IReadOnlyList<StrengthSession> sessions, HistoryRange range)
    {
        LiftSummaries.Add((sessions, range));
        return Summary(sessions, range);
    }

    /// <inheritdoc />
    public BodyweightProgress SummariseBodyweight(IReadOnlyList<StrengthSession> sessions, HistoryRange range)
    {
        BodyweightSummaries.Add((sessions, range));
        return BodyweightSummary(sessions, range);
    }

    /// <inheritdoc />
    public AssistedProgress SummariseAssisted(IReadOnlyList<StrengthSession> sessions, HistoryRange range)
    {
        AssistedSummaries.Add((sessions, range));
        return AssistedSummary(sessions, range);
    }

    /// <inheritdoc />
    public HoldProgress SummariseHold(IReadOnlyList<StrengthSession> sessions, HistoryRange range)
    {
        HoldSummaries.Add((sessions, range));
        return HoldSummary(sessions, range);
    }

    /// <inheritdoc />
    public CarryProgress SummariseCarry(IReadOnlyList<StrengthSession> sessions, HistoryRange range)
    {
        CarrySummaries.Add((sessions, range));
        return CarrySummary(sessions, range);
    }

    /// <summary>Completes the latest held load with <see cref="Result"/>.</summary>
    public void Complete() => pending[^1].SetResult(Result);
}
