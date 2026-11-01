using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Services;

/// <summary>
/// An exercise's saved sessions, ready to tell which entry of a workout being logged beats the exercise's best in every saved workout.
/// Loaded by <see cref="IRecordBaselineService"/>.
/// </summary>
public sealed class RecordBaseline
{
    private readonly ExerciseDefinition exercise;
    private readonly IReadOnlyList<WorkoutSession> saved;
    private readonly IBestRule rule;
    private readonly IRecordCheck recordCheck;

    internal RecordBaseline(ExerciseDefinition exercise, IReadOnlyList<WorkoutSession> saved, IBestRule rule, IRecordCheck recordCheck)
    {
        this.exercise = exercise;
        this.saved = saved;
        this.rule = rule;
        this.recordCheck = recordCheck;
    }

    /// <summary>
    /// Returns the index of the set (strength) or entry (cardio) in <paramref name="current"/> that gives the session's headline best,
    /// if that best beats every saved session; otherwise null. A tie for the best goes to the earlier entry. A first session or a tie
    /// with the saved best is no record. <paramref name="current"/> holds valid entries only.
    /// </summary>
    public int? RecordEntryIndex(WorkoutExercise current)
    {
        ArgumentNullException.ThrowIfNull(current);
        var entries = Split(current);
        int? winner = null;
        ExerciseBest? winning = null;
        for (var i = 0; i < entries.Count; i++)
        {
            if (rule.Best(exercise, entries[i]) is { } best && (winning is null || rule.Compare(best, winning) > 0))
            {
                winner = i;
                winning = best;
            }
        }

        if (winner is not { } index)
        {
            return null;
        }

        var candidate = new WorkoutSession(0, DateTimeOffset.MaxValue, entries[index]);
        var latest = recordCheck.Records(exercise, saved.Append(candidate)).LastOrDefault();
        return latest?.Session == candidate ? index : null;
    }

    private static List<WorkoutExercise> Split(WorkoutExercise current) => current switch
    {
        StrengthExercise strength => strength.Sets.Select(set => (WorkoutExercise)new StrengthExercise(strength.ExerciseId, [set])).ToList(),
        _ => [current],
    };
}
