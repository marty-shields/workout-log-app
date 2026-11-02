namespace WorkoutLog.UI.Models.Workout;

/// <summary>Checks a whole workout on Save.</summary>
public interface IWorkoutChecker
{
    /// <summary>
    /// Checks every set and entry, the workout and its note, and each exercise and its note. Sets <see cref="ExerciseState.Error"/>,
    /// <see cref="ExerciseState.NoteError"/> and <see cref="EntryState.Errors"/>, and returns the workout's own messages.
    /// </summary>
    /// <param name="exercises">The exercises, newest first.</param>
    /// <param name="note">The workout's note as typed.</param>
    WorkoutCheck Check(IReadOnlyList<ExerciseState> exercises, string? note = null);

    /// <summary>
    /// Checks one exercise as <see cref="Check"/> does: every set and entry, the exercise's note and its own rule such as at least one set.
    /// Sets <see cref="ExerciseState.Error"/>, <see cref="ExerciseState.NoteError"/> and <see cref="EntryState.Errors"/> and returns false if anything fails. The rules
    /// across the workout, a repeated exercise and the set limit, are left to <see cref="Check"/>.
    /// </summary>
    bool CheckExercise(ExerciseState exercise);

    /// <summary>Message on the workout when a save fails.</summary>
    string SaveFailed { get; }

    /// <summary>Message on the workout when a delete fails.</summary>
    string DeleteFailed { get; }
}

/// <summary>Outcome of <see cref="IWorkoutChecker.Check"/>.</summary>
/// <param name="IsValid">True when nothing failed.</param>
/// <param name="Error">Message on the workout itself, e.g. when it has no exercises.</param>
/// <param name="NoteError">Message on the workout's note, e.g. when it is too long.</param>
public sealed record WorkoutCheck(bool IsValid, string? Error, string? NoteError = null);
