namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>Compares an exercise's best in a session with its best in an earlier one, by the exercise's <see cref="IBestRule"/>.</summary>
internal interface IBestChangeRule
{
    /// <summary>
    /// Returns <see cref="BestChangeKind.FirstTime"/> when <paramref name="previous"/> is null. Otherwise Up when <paramref name="current"/> beats it,
    /// Down when it beats <paramref name="current"/> and Equal on a tie. The difference is the first of weight, reps, duration and distance that differs.
    /// </summary>
    BestChange Change(ExerciseDefinition exercise, ExerciseBest current, ExerciseBest? previous);
}
