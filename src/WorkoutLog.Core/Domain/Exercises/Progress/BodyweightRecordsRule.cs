using static WorkoutLog.Core.Domain.Exercises.Progress.RecordPick;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>
/// Most reps in a set: the highest best set. Most reps in a session: the highest total. Heaviest weight and best e1RM: the lift records rule.
/// A tie across sessions takes the earliest start.
/// </summary>
internal sealed class BodyweightRecordsRule(ILiftRecordsRule liftRecordsRule) : IBodyweightRecordsRule
{
    /// <inheritdoc />
    public BodyweightRecords Records(IReadOnlyList<BodyweightSessionProgress> sessions)
    {
        var lift = liftRecordsRule.Records([.. sessions.Select(s => new LiftSessionProgress(s.Session, s.TopSet, s.E1rmKg))]);
        return new BodyweightRecords(
            Highest(sessions, s => s.Session.StartedAt, s => s.BestSetReps),
            Highest(sessions, s => s.Session.StartedAt, s => s.TotalReps),
            lift.HeaviestWeight,
            lift.BestE1rmKg);
    }
}
