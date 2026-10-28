using WorkoutLog.Core.Domain.Exercises.Bests;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Least assist: the lowest session least assist; tie → most reps. A tie across sessions takes the earliest start.</summary>
internal sealed class AssistedRecordsRule : IAssistedRecordsRule
{
    /// <inheritdoc />
    public AssistedRecords Records(IReadOnlyList<AssistedSessionProgress> sessions)
    {
        var byStart = sessions.OrderBy(s => s.Session.StartedAt).ToList();
        var least = byStart.Where(s => s.LeastAssist is not null)
            .OrderBy(s => s.LeastAssist!.WeightKg)
            .ThenByDescending(s => s.LeastAssist!.Reps)
            .Select(s => new ProgressRecord<ExerciseBest>(s.LeastAssist!, s.Session.StartedAt))
            .FirstOrDefault();
        return new AssistedRecords(least);
    }
}
