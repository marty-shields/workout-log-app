using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Tests;

/// <summary>
/// <see cref="IRecordBaselineService"/> for the tests: no baseline, so no badges, until a test sets a baseline built from saved
/// sessions with <see cref="SetSaved"/> or a failure with <see cref="SetFailed"/>. The baselines use the real rules.
/// </summary>
public sealed class FakeRecordBaselineService : IRecordBaselineService
{
    private static readonly DateTimeOffset Start = new(2026, 10, 1, 18, 0, 0, TimeSpan.Zero);

    private static readonly IBestRule[] Rules =
    [
        new LiftBestRule(new TopSetRule()), new BodyweightBestRule(new BodyweightRule(new TopSetRule(), new E1rmRule())), new AssistedBestRule(new LeastAssistRule()),
        new HoldBestRule(new HoldRule()), new CarryBestRule(new CarryRule()), new CardioBestRule(new CardioSessionRule()),
    ];

    private readonly Dictionary<int, RecordBaselineResult> results = [];

    /// <summary>The exercise id of each load, in order.</summary>
    public List<int> Loads { get; } = [];

    /// <summary>The exercise's saved workouts, oldest first.</summary>
    public void SetSaved(IExerciseCatalog catalog, int exerciseId, params WorkoutExercise[] saved)
    {
        var exercise = catalog.All.Single(e => e.Id == exerciseId);
        var sessions = saved.Select((logged, i) => new WorkoutSession(i + 1, Start.AddDays(i), logged)).ToList();
        results[exerciseId] = new RecordBaselineResult(
            new RecordBaseline(exercise, sessions, Rules.Single(r => r.AppliesTo(exercise)), new RecordCheck(Rules)), Failed: false);
    }

    /// <summary>The load of the exercise fails.</summary>
    public void SetFailed(int exerciseId) => results[exerciseId] = new RecordBaselineResult(null, Failed: true);

    /// <inheritdoc />
    public Task<RecordBaselineResult> LoadAsync(int exerciseId)
    {
        Loads.Add(exerciseId);
        return Task.FromResult(results.GetValueOrDefault(exerciseId, new RecordBaselineResult(null, Failed: false)));
    }
}
