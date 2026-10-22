using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Domain.Workouts.Validation;

/// <summary>Errors of one strength set or cardio entry.</summary>
/// <param name="Fields">Error per metric at fault. Empty when valid.</param>
public sealed record EntryErrors(IReadOnlyDictionary<Metric, FieldError> Fields)
{
    /// <summary>True when no metric is at fault.</summary>
    public bool IsValid => Fields.Count == 0;
}
