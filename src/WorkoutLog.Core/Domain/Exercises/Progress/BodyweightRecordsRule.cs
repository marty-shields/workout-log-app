namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>
/// Most reps in a set: the highest best set. Most reps in a session: the highest total. Heaviest weight and best e1RM: the lift records rule.
/// A tie across sessions takes the earliest start.
/// </summary>
internal sealed class BodyweightRecordsRule(IRecordsRule liftRecordsRule) : IBodyweightRecordsRule
{
    /// <inheritdoc />
    public BodyweightRecords Records(IReadOnlyList<BodyweightSessionProgress> sessions)
    {
        var byStart = sessions.OrderBy(s => s.Session.StartedAt).ToList();
        var lift = liftRecordsRule.Records(byStart.Select(s => new LiftSessionProgress(s.Session, s.TopSet, s.E1rmKg)).ToList());
        return new BodyweightRecords(Most(byStart, s => s.BestSetReps), Most(byStart, s => s.TotalReps), lift.HeaviestWeight, lift.BestE1rmKg);
    }

    private static ProgressRecord<int>? Most(List<BodyweightSessionProgress> byStart, Func<BodyweightSessionProgress, int?> reps) =>
        byStart.Where(s => reps(s) is not null)
            .OrderByDescending(reps)
            .Select(s => new ProgressRecord<int>(reps(s)!.Value, s.Session.StartedAt))
            .FirstOrDefault();
}
