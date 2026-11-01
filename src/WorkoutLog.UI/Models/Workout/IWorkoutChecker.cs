namespace WorkoutLog.UI.Models.Workout;

/// <summary>Checks a whole workout on Save.</summary>
public interface IWorkoutChecker
{
    /// <summary>
    /// Checks every set and entry, and the workout and each exercise. Sets <see cref="ExerciseState.Error"/> and
    /// <see cref="EntryState.Errors"/>, and returns the workout's own message.
    /// </summary>
    WorkoutCheck Check(IReadOnlyList<ExerciseState> exercises);

    /// <summary>
    /// Checks one exercise as <see cref="Check"/> does: every set and entry, and the exercise's own rule such as at least one set.
    /// Sets <see cref="ExerciseState.Error"/> and <see cref="EntryState.Errors"/> and returns false if anything fails. The rules
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
public sealed record WorkoutCheck(bool IsValid, string? Error);
