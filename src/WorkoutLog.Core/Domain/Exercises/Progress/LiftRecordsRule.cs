using static WorkoutLog.Core.Domain.Exercises.Progress.RecordPick;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>
/// Heaviest weight: the heaviest top set; tie → most reps. Best e1RM: the highest unrounded estimate.
/// A tie across sessions takes the earliest start.
/// </summary>
internal sealed class LiftRecordsRule : ILiftRecordsRule
{
    /// <inheritdoc />
    public LiftRecords Records(IReadOnlyList<LiftSessionProgress> sessions) => new(
        First(sessions, s => s.Session.StartedAt, s => s.TopSet, r => r.OrderByDescending(x => x.Value.WeightKg).ThenByDescending(x => x.Value.Reps)),
        Highest(sessions, s => s.Session.StartedAt, s => s.E1rmKg));
}
