using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>Compares each session's best with the best so far, by start time, with the exercise's <see cref="IBestRule"/>.</summary>
internal sealed class RecordCheck(IEnumerable<IBestRule> rules) : IRecordCheck
{
    private readonly IReadOnlyList<IBestRule> rules = rules.ToList();

    /// <inheritdoc />
    public IReadOnlyList<SessionRecord> Records(ExerciseDefinition exercise, IEnumerable<WorkoutSession> sessions)
    {
        var rule = rules.Single(r => r.AppliesTo(exercise));
        var records = new List<SessionRecord>();
        ExerciseBest? bestSoFar = null;
        foreach (var session in sessions.OrderBy(s => s.StartedAt))
        {
            if (rule.Best(exercise, session.Exercise) is not { } best)
            {
                continue;
            }

            if (bestSoFar is null)
            {
                bestSoFar = best;
            }
            else if (rule.Compare(best, bestSoFar) > 0)
            {
                records.Add(new SessionRecord(session, best));
                bestSoFar = best;
            }
        }

        return records;
    }
}
