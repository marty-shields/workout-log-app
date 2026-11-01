using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>Finds the direction with the exercise's <see cref="IBestRule"/>, so it matches the Exercises list and the record check.</summary>
internal sealed class BestChangeRule(IEnumerable<IBestRule> rules) : IBestChangeRule
{
    private readonly IReadOnlyList<IBestRule> rules = rules.ToList();

    /// <inheritdoc />
    public SessionChange? Change(ExerciseDefinition exercise, WorkoutExercise current, IEnumerable<WorkoutExercise> earlier)
    {
        var rule = rules.Single(r => r.AppliesTo(exercise));
        if (rule.Best(exercise, current) is not { } best)
        {
            return null;
        }

        var previous = earlier.Select(e => rule.Best(exercise, e)).OfType<ExerciseBest>().LastOrDefault();
        if (previous is null)
        {
            return new SessionChange(best, new BestChange(BestChangeKind.FirstTime, null));
        }

        var order = rule.Compare(best, previous);
        return new SessionChange(best, order == 0
            ? new BestChange(BestChangeKind.Equal, null)
            : new BestChange(order > 0 ? BestChangeKind.Up : BestChangeKind.Down, Difference(best, previous)));
    }

    private static ExerciseBest Difference(ExerciseBest x, ExerciseBest y)
    {
        var weight = Math.Abs((x.WeightKg ?? 0m) - (y.WeightKg ?? 0m));
        if (weight != 0m)
        {
            return new ExerciseBest(weight, null, null, null);
        }

        var reps = Math.Abs((x.Reps ?? 0) - (y.Reps ?? 0));
        if (reps != 0)
        {
            return new ExerciseBest(null, reps, null, null);
        }

        var duration = ((x.Duration ?? TimeSpan.Zero) - (y.Duration ?? TimeSpan.Zero)).Duration();
        if (duration != TimeSpan.Zero)
        {
            return new ExerciseBest(null, null, duration, null);
        }

        var distance = Math.Abs((x.Distance ?? 0m) - (y.Distance ?? 0m));
        return distance != 0m
            ? new ExerciseBest(null, null, null, distance)
            : throw new InvalidOperationException("The bests differ in no field the summary can show.");
    }
}
