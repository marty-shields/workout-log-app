namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>Finds the direction with the exercise's <see cref="IBestRule"/>, so it matches the Exercises list and the record check.</summary>
internal sealed class BestChangeRule(IEnumerable<IBestRule> rules) : IBestChangeRule
{
    private readonly IReadOnlyList<IBestRule> rules = rules.ToList();

    /// <inheritdoc />
    public BestChange Change(ExerciseDefinition exercise, ExerciseBest current, ExerciseBest? previous)
    {
        if (previous is null)
        {
            return new BestChange(BestChangeKind.FirstTime, null);
        }

        var order = rules.Single(r => r.AppliesTo(exercise)).Compare(current, previous);
        if (order == 0)
        {
            return new BestChange(BestChangeKind.Equal, null);
        }

        return new BestChange(order > 0 ? BestChangeKind.Up : BestChangeKind.Down, Difference(current, previous));
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

        var duration = (x.Duration ?? TimeSpan.Zero) - (y.Duration ?? TimeSpan.Zero);
        if (duration != TimeSpan.Zero)
        {
            return new ExerciseBest(null, null, duration.Duration(), null);
        }

        return new ExerciseBest(null, null, null, Math.Abs((x.Distance ?? 0m) - (y.Distance ?? 0m)));
    }
}
