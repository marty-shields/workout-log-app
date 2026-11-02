using static WorkoutLog.Core.Domain.Exercises.Progress.RecordPick;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Least assist: the lowest session least assist; tie → most reps. A tie across sessions takes the earliest start.</summary>
internal sealed class AssistedRecordsRule
{
    /// <summary>The records of <paramref name="sessions"/>.</summary>
    public AssistedRecords Records(IReadOnlyList<AssistedSessionProgress> sessions) => new(
        First(sessions, s => s.Session.StartedAt, s => s.LeastAssist, r => r.OrderBy(x => x.Value.WeightKg).ThenByDescending(x => x.Value.Reps)));
}
