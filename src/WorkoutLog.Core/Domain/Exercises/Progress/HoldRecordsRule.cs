using static WorkoutLog.Core.Domain.Exercises.Progress.RecordPick;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>
/// Longest hold: the longest session duration. Heaviest weight: the heaviest session weight; tie → longest.
/// A tie across sessions takes the earliest start.
/// </summary>
internal sealed class HoldRecordsRule : IHoldRecordsRule
{
    /// <inheritdoc />
    public HoldRecords Records(IReadOnlyList<HoldSessionProgress> sessions) => new(
        Highest(sessions, s => s.Session.StartedAt, s => s.Longest),
        First(sessions, s => s.Session.StartedAt, s => s.Heaviest, r => r.OrderByDescending(x => x.Value.WeightKg).ThenByDescending(x => x.Value.Duration)));
}
