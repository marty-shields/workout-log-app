using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.UI.Models.Workout;

/// <summary>Checks the entered text of one strength set or cardio entry against the exercise's rules (docs/03).</summary>
public interface ISetChecker
{
    /// <summary>
    /// Fills <see cref="EntryState.Errors"/> with one message per field at fault and returns false if any field fails.
    /// An exercise code (unknown exercise or wrong type) also fails it, with no message, and is logged.
    /// </summary>
    bool Check(ExerciseState exercise, EntryState entry);

    /// <summary>As <see cref="Check"/>, but an exercise code isn't logged: <see cref="IWorkoutChecker"/> words and logs it on Save.</summary>
    bool CheckOnSave(ExerciseState exercise, EntryState entry);

    /// <summary>The exercise as Core records, read from the entered text. Fields that can't be read are left empty, as is a cardio exercise with no entry.</summary>
    WorkoutExercise Read(ExerciseState exercise);
}
