using WorkoutLog.Core.Domain.Exercises.Bests;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>
/// Heaviest weight: the heaviest top set; tie → most reps. Best e1RM: the highest unrounded estimate.
/// A tie across sessions takes the earliest start.
/// </summary>
internal sealed class RecordsRule : IRecordsRule
{
    /// <inheritdoc />
    public LiftRecords Records(IReadOnlyList<LiftSessionProgress> sessions)
    {
        var byStart = sessions.OrderBy(s => s.Session.StartedAt).ToList();
        var heaviest = byStart.Where(s => s.TopSet is not null)
            .OrderByDescending(s => s.TopSet!.WeightKg)
            .ThenByDescending(s => s.TopSet!.Reps)
            .Select(s => new ProgressRecord<ExerciseBest>(s.TopSet!, s.Session.StartedAt))
            .FirstOrDefault();
        var e1rm = byStart.Where(s => s.E1rmKg is not null)
            .OrderByDescending(s => s.E1rmKg)
            .Select(s => new ProgressRecord<decimal>(s.E1rmKg!.Value, s.Session.StartedAt))
            .FirstOrDefault();
        return new LiftRecords(heaviest, e1rm);
    }
}
