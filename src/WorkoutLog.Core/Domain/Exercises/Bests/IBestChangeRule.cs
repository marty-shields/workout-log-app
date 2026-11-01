using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>Compares an exercise's best in a session with its best in the last earlier session, by the exercise's <see cref="IBestRule"/>.</summary>
internal interface IBestChangeRule
{
    /// <summary>
    /// Returns the best of <paramref name="current"/> and how it compares with the last of <paramref name="earlier"/> (oldest first) that has a best,
    /// or null when <paramref name="current"/> has no best. <see cref="BestChangeKind.FirstTime"/> when no earlier session has one. Otherwise Up when the
    /// best beats the earlier one, Down when it is beaten and Equal on a tie. The difference is the first of weight, reps, duration and distance that differs.
    /// </summary>
    SessionChange? Change(ExerciseDefinition exercise, WorkoutExercise current, IEnumerable<WorkoutExercise> earlier);
}
