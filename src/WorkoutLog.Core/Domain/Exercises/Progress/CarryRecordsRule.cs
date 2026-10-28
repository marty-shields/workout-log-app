using WorkoutLog.Core.Domain.Exercises.Bests;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>
/// Heaviest weight: the heaviest session weight; tie → longest. Longest distance: the longest session distance; tie → heaviest.
/// A tie across sessions takes the earliest start.
/// </summary>
internal sealed class CarryRecordsRule : ICarryRecordsRule
{
    /// <inheritdoc />
    public CarryRecords Records(IReadOnlyList<CarrySessionProgress> sessions)
    {
        var byStart = sessions.OrderBy(s => s.Session.StartedAt).ToList();
        var heaviest = byStart.Where(s => s.Heaviest is not null)
            .OrderByDescending(s => s.Heaviest!.WeightKg)
            .ThenByDescending(s => s.Heaviest!.Distance)
            .Select(s => new ProgressRecord<ExerciseBest>(s.Heaviest!, s.Session.StartedAt))
            .FirstOrDefault();
        var longest = byStart.Where(s => s.Longest is not null)
            .OrderByDescending(s => s.Longest!.Distance)
            .ThenByDescending(s => s.Longest!.WeightKg ?? 0m)
            .Select(s => new ProgressRecord<ExerciseBest>(s.Longest!, s.Session.StartedAt))
            .FirstOrDefault();
        return new CarryRecords(heaviest, longest);
    }
}
