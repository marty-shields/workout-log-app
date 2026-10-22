namespace WorkoutLog.Core.Domain.Workouts.Validation;

/// <summary>Errors of one exercise in a workout.</summary>
/// <param name="Error">Error on the exercise itself, or null.</param>
/// <param name="Entries">Errors per set (strength) or of the entry (cardio), in order. Empty when the exercise is unknown or the wrong type.</param>
public sealed record ExerciseErrors(ExerciseError? Error, IReadOnlyList<EntryErrors> Entries)
{
    /// <summary>True when the exercise and all its sets or its entry are valid.</summary>
    public bool IsValid => Error is null && Entries.All(e => e.IsValid);
}
