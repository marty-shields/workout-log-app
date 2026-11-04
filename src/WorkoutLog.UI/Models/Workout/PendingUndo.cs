using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.UI.Models.Workout;

/// <summary>The latest deletion, which Undo puts back. At most one at a time: the next deletion replaces it and keeps the earlier one deleted.</summary>
/// <param name="Message">What the snackbar says, e.g. Barbell Back Squat set 2 deleted.</param>
public abstract record PendingUndo(string Message)
{
    /// <summary>Putting it back keeps the workout within <see cref="WorkoutLimits"/>.</summary>
    /// <param name="exercises">The workout's exercises now.</param>
    public abstract bool Fits(IReadOnlyCollection<ExerciseState> exercises);
}

/// <summary>An exercise taken out of the workout.</summary>
/// <param name="Index">Its position in screen order, from 0.</param>
public sealed record RemovedExercise(ExerciseState Exercise, int Index) : PendingUndo($"{Exercise.Name} deleted")
{
    /// <summary>Also needs the exercise to be out of the workout: picked again, it would be in it twice.</summary>
    public override bool Fits(IReadOnlyCollection<ExerciseState> exercises) =>
        exercises.Count < WorkoutLimits.MaxExercises && exercises.All(e => e.ExerciseId != Exercise.ExerciseId);
}

/// <summary>A closed set taken out of its exercise.</summary>
/// <param name="Above">The sets above it when it was deleted, top first. Undo puts it back by these, not by number, so a set removed or added meanwhile doesn't move it.</param>
public sealed record RemovedSet(ExerciseState Exercise, EntryState Entry, IReadOnlyList<EntryState> Above) : PendingUndo($"{Exercise.Name} set {Above.Count + 1} deleted")
{
    /// <summary>Where Undo puts it back, from 0: just under the lowest of <see cref="Above"/> still in the exercise, or first when none is.</summary>
    public int Index => Exercise.Entries.FindLastIndex(Above.Contains) + 1;

    public override bool Fits(IReadOnlyCollection<ExerciseState> exercises) => !Exercise.SetsFull;
}
