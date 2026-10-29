namespace WorkoutLog.Core.Domain.Workouts.Validation;

/// <summary>Errors of a workout.</summary>
/// <param name="Errors">Every error on the workout itself. Empty when there is none.</param>
/// <param name="Exercises">Errors per exercise, in order.</param>
public sealed record WorkoutErrors(IReadOnlyList<WorkoutError> Errors, IReadOnlyList<ExerciseErrors> Exercises)
{
    /// <summary>True when the workout and all its exercises are valid.</summary>
    public bool IsValid => Errors.Count == 0 && Exercises.All(e => e.IsValid);
}
