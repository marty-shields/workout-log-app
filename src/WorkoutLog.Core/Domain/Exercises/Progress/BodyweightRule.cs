namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>
/// Best set and total use the unweighted sets: 0 kg. Top set and e1RM use the sets over 0 kg, with the lift rules.
/// </summary>
internal sealed class BodyweightRule(ITopSetRule topSetRule, IE1rmRule e1rmRule) : IBodyweightRule
{
    /// <inheritdoc />
    public BodyweightSessionProgress Progress(StrengthSession session)
    {
        var weighted = session.Sets.Where(s => s.WeightKg > 0m).ToList();
        var unweightedReps = session.Sets.Where(s => s is { Reps: not null } && !(s.WeightKg > 0m)).Select(s => s.Reps!.Value).ToList();
        return new BodyweightSessionProgress(
            session,
            unweightedReps.Count == 0 ? null : unweightedReps.Max(),
            unweightedReps.Count == 0 ? null : unweightedReps.Sum(),
            topSetRule.TopSet(weighted),
            e1rmRule.Best(weighted));
    }
}
