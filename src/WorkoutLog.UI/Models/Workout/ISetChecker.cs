namespace WorkoutLog.UI.Models.Workout;

/// <summary>Checks the entered text of one strength set against the exercise's rules (docs/03).</summary>
public interface ISetChecker
{
    /// <summary>
    /// Fills <see cref="EntryState.Errors"/> with one message per field at fault and returns false if any field fails.
    /// A cardio entry passes with no checks.
    /// </summary>
    bool Check(ExerciseState exercise, EntryState entry);
}
