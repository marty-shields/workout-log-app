using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Models.Workout;

/// <summary>
/// Checks each set and entry with <see cref="ISetChecker"/>, then the records it reads with <see cref="IWorkoutService.Validate"/>,
/// and words the workout and exercise codes. An unknown exercise or wrong type is logged once per exercise.
/// </summary>
public sealed partial class WorkoutChecker(ISetChecker sets, IWorkoutService workouts, ILogger<WorkoutChecker> logger) : IWorkoutChecker
{
    /// <inheritdoc />
    public WorkoutCheck Check(IReadOnlyList<ExerciseState> exercises)
    {
        var valid = true;
        foreach (var exercise in exercises)
        {
            foreach (var entry in exercise.Entries)
            {
                valid &= sets.CheckOnSave(exercise, entry);
            }
        }

        var result = workouts.Validate(exercises.Select(sets.Read).ToList());
        for (var i = 0; i < exercises.Count; i++)
        {
            var code = result.Exercises[i].Error;
            exercises[i].ErrorCode = code;
            exercises[i].Error = Message(code);
            if (code is ExerciseError.UnknownExercise or ExerciseError.WrongType)
            {
                Log.ExerciseError(logger, exercises[i].ExerciseId, code.Value);
            }
        }

        valid &= result.IsValid;
        if (valid)
        {
            foreach (var entry in exercises.SelectMany(e => e.Entries))
            {
                entry.CheckOnChange = false;
            }
        }

        return new WorkoutCheck(valid, Message(result.Error));
    }

    /// <inheritdoc />
    public string SaveFailed => Message(WorkoutError.SaveFailed)!;

    /// <inheritdoc />
    public string DeleteFailed => "Couldn't delete the workout. Try again.";

    private static string? Message(WorkoutError? code) => code switch
    {
        WorkoutError.NoExercises => "Add at least one exercise",
        WorkoutError.SaveFailed => "Couldn't save the workout. Try again.",
        _ => null,
    };

    private static string? Message(ExerciseError? code) => code switch
    {
        ExerciseError.UnknownExercise => "Exercise not found",
        ExerciseError.WrongType => "Exercise type doesn't match",
        ExerciseError.NoSets => "Add at least one set",
        _ => null,
    };

    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.SaveCheckExerciseError, LogLevel.Warning, "Save check for exercise {ExerciseId} failed with {Error}")]
        public static partial void ExerciseError(ILogger logger, int exerciseId, ExerciseError error);
    }
}
