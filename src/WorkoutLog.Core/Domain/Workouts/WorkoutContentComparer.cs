using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>Compares workouts as their export would write them. A blank weight on an assisted machine counts as 0 (D23).</summary>
internal sealed class WorkoutContentComparer(IExerciseCatalog catalog) : IWorkoutContentComparer
{
    /// <inheritdoc />
    public bool AreSame(Workout saved, NewWorkout imported) =>
        saved.FinishedAt.UtcTicks == imported.FinishedAt.UtcTicks
        && saved.Exercises.Count == imported.Exercises.Count
        && saved.Exercises.Zip(imported.Exercises).All(pair => AreSame(pair.First, pair.Second));

    private bool AreSame(WorkoutExercise saved, WorkoutExercise imported) =>
        saved.ExerciseId == imported.ExerciseId
        && (saved, imported) switch
        {
            (StrengthExercise a, StrengthExercise b) => AreSame(a, b),
            (CardioExercise a, CardioExercise b) => AreSame(a.Entry, b.Entry),
            _ => false,
        };

    private bool AreSame(StrengthExercise a, StrengthExercise b)
    {
        var blankKg = catalog.IsAssisted(a.ExerciseId) ? 0m : (decimal?)null;
        return a.Sets.Count == b.Sets.Count && a.Sets.Zip(b.Sets).All(s => AreSame(s.First, s.Second, blankKg));
    }

    /// <summary>Compares two sets; a blank weight counts as <paramref name="blankKg"/>.</summary>
    private static bool AreSame(StrengthSet a, StrengthSet b, decimal? blankKg) =>
        a.Reps == b.Reps
        && Same(a.WeightKg ?? blankKg, b.WeightKg ?? blankKg)
        && Same(a.Duration, b.Duration)
        && Same(a.Distance, b.Distance);

    private static bool AreSame(CardioEntry a, CardioEntry b) =>
        Same(a.Duration, b.Duration)
        && Same(a.Distance, b.Distance)
        && a.Floors == b.Floors
        && a.Level == b.Level
        && Same(a.Incline, b.Incline)
        && a.Calories == b.Calories
        && Same(a.Rate, b.Rate)
        && Same(a.Pace, b.Pace);

    private static bool Same(decimal? a, decimal? b) =>
        (a, b) switch
        {
            (null, null) => true,
            ({ } x, { } y) => Decimals.Round2(x) == Decimals.Round2(y),
            _ => false,
        };

    private static bool Same(TimeSpan? a, TimeSpan? b) =>
        (a, b) switch
        {
            (null, null) => true,
            ({ } x, { } y) => (long)x.TotalSeconds == (long)y.TotalSeconds,
            _ => false,
        };
}
