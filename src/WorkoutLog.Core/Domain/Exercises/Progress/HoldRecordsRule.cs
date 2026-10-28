using WorkoutLog.Core.Domain.Exercises.Bests;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>
/// Longest hold: the longest session duration. Heaviest weight: the heaviest session weight; tie → longest.
/// A tie across sessions takes the earliest start.
/// </summary>
internal sealed class HoldRecordsRule : IHoldRecordsRule
{
    /// <inheritdoc />
    public HoldRecords Records(IReadOnlyList<HoldSessionProgress> sessions)
    {
        var byStart = sessions.OrderBy(s => s.Session.StartedAt).ToList();
        var longest = byStart.Where(s => s.Longest is not null)
            .OrderByDescending(s => s.Longest)
            .Select(s => new ProgressRecord<TimeSpan>(s.Longest!.Value, s.Session.StartedAt))
            .FirstOrDefault();
        var heaviest = byStart.Where(s => s.Heaviest is not null)
            .OrderByDescending(s => s.Heaviest!.WeightKg)
            .ThenByDescending(s => s.Heaviest!.Duration)
            .Select(s => new ProgressRecord<ExerciseBest>(s.Heaviest!, s.Session.StartedAt))
            .FirstOrDefault();
        return new HoldRecords(longest, heaviest);
    }
}
