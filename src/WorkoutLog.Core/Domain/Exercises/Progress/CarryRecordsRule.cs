using static WorkoutLog.Core.Domain.Exercises.Progress.RecordPick;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>
/// Heaviest weight: the heaviest session weight; tie → longest. Longest distance: the longest session distance; tie → heaviest.
/// A tie across sessions takes the earliest start.
/// </summary>
internal sealed class CarryRecordsRule : ICarryRecordsRule
{
    /// <inheritdoc />
    public CarryRecords Records(IReadOnlyList<CarrySessionProgress> sessions) => new(
        First(sessions, s => s.Session.StartedAt, s => s.Heaviest, r => r.OrderByDescending(x => x.Value.WeightKg).ThenByDescending(x => x.Value.Distance)),
        First(sessions, s => s.Session.StartedAt, s => s.Longest, r => r.OrderByDescending(x => x.Value.Distance).ThenByDescending(x => x.Value.WeightKg ?? 0m)));
}
