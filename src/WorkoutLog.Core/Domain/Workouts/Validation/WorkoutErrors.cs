namespace WorkoutLog.Core.Domain.Workouts.Validation;

/// <summary>Errors of a workout.</summary>
/// <param name="Error">Error on the workout itself, or null.</param>
/// <param name="Exercises">Errors per exercise, in order.</param>
public sealed record WorkoutErrors(WorkoutError? Error, IReadOnlyList<ExerciseErrors> Exercises)
{
    /// <summary>True when the workout and all its exercises are valid.</summary>
    public bool IsValid => Error is null && Exercises.All(e => e.IsValid);
}
