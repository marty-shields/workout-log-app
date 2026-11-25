using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Models.Workout;

/// <summary>Turns the exercises on screen into an <see cref="UnfinishedWorkout"/> and back.</summary>
public static class UnfinishedWorkouts
{
    /// <summary>
    /// The workout as typed: each set's text, notes and done state, with the exercises oldest first. Validation messages,
    /// the last time and the records are not kept, as they are read or checked again.
    /// </summary>
    /// <param name="exercises">In screen order, newest first.</param>
    public static UnfinishedWorkout From(DateTimeOffset startedAt, string? note, IEnumerable<ExerciseState> exercises) =>
        new(startedAt, note, [.. exercises.Reverse().Select(e => new UnfinishedExercise(
            e.ExerciseId,
            e.Note,
            e.IsDone,
            [.. e.Entries.Select(entry => new UnfinishedEntry(new Dictionary<string, string?>(entry.Values)))]))]);

    /// <summary>
    /// The exercises of <paramref name="workout"/>, oldest first, as <see cref="WorkoutEditor.Start"/> takes them. Every exercise must be in the
    /// exercise list, as <see cref="IUnfinishedWorkoutService"/> leaves out the ones that aren't.
    /// </summary>
    public static IReadOnlyList<ExerciseState> ToExercises(UnfinishedWorkout workout, IExerciseCatalog catalog) =>
        [.. workout.Exercises.Select(saved => ExerciseFields.Restored(catalog.All.Single(d => d.Id == saved.ExerciseId), saved))];
}
