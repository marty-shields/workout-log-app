namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>The fixed exercise list.</summary>
public interface IExerciseCatalog
{
    /// <summary>Every exercise, in Id order.</summary>
    IReadOnlyList<ExerciseDefinition> All { get; }

    /// <summary>
    /// Exercises whose name or nicknames contain a word starting with each query word, in any order (docs/03).
    /// Name starting with the query words first, then other name matches, then nickname matches; A–Z within each.
    /// An empty query returns every exercise, Strength then Cardio, A–Z. Text past <see cref="ExerciseCatalog.MaxQueryLength"/> is ignored.
    /// </summary>
    IReadOnlyList<ExerciseDefinition> Search(string? query);
}
