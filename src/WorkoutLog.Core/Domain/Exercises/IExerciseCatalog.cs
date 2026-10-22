namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>The fixed exercise list.</summary>
public interface IExerciseCatalog
{
    /// <summary>Every exercise, in Id order.</summary>
    IReadOnlyList<ExerciseDefinition> All { get; }
}
