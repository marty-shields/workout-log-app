namespace WorkoutLog.Core.Domain.Workouts.Validation;

/// <param name="Errors">Every error on the workout itself. Empty when there is none.</param>
/// <param name="Exercises">Errors per exercise, in order.</param>
/// <param name="Note">Error on the workout's note, or null.</param>
public sealed record WorkoutErrors(IReadOnlyList<WorkoutError> Errors, IReadOnlyList<ExerciseErrors> Exercises, NoteError? Note = null)
{
    /// <summary>True when the workout, its note and all its exercises are valid.</summary>
    public bool IsValid => Errors.Count == 0 && Note is null && Exercises.All(e => e.IsValid);
}
