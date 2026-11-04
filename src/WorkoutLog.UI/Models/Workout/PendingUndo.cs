namespace WorkoutLog.UI.Models.Workout;

/// <summary>The latest deletion, which Undo puts back. At most one at a time: the next deletion replaces it and keeps the earlier one deleted.</summary>
/// <param name="Message">What the snackbar says, e.g. Set 2 deleted.</param>
public abstract record PendingUndo(string Message);

/// <summary>An exercise taken out of the workout.</summary>
/// <param name="Index">Its position in screen order, from 0.</param>
public sealed record RemovedExercise(ExerciseState Exercise, int Index) : PendingUndo($"{Exercise.Name} deleted");

/// <summary>A closed set taken out of its exercise.</summary>
/// <param name="Index">Its position in the exercise's sets, from 0.</param>
public sealed record RemovedSet(ExerciseState Exercise, EntryState Entry, int Index) : PendingUndo($"Set {Index + 1} deleted");
