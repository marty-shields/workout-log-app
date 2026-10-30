namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>The fixed exercise list.</summary>
public interface IExerciseCatalog
{
    /// <summary>Every exercise, in Id order.</summary>
    IReadOnlyList<ExerciseDefinition> All { get; }

    /// <summary>
    /// Exercises whose name or nicknames contain a word starting with each query word, in any order.
    /// Name starting with the query words first, then other name matches, then nickname matches; A–Z within each.
    /// An empty query returns every exercise, Strength then Cardio, A–Z. Text past <see cref="ExerciseCatalog.MaxQueryLength"/> is ignored.
    /// </summary>
    IReadOnlyList<ExerciseDefinition> Search(string? query);

    /// <summary>True when <paramref name="query"/> has a word to search for, as <see cref="Search"/> reads it; false for none, spaces or punctuation only.</summary>
    bool HasSearchWords(string? query);

    /// <summary>True when the exercise with <paramref name="exerciseId"/> is an assisted machine (<see cref="ExerciseDefinition.IsAssisted"/>); false for an unknown id.</summary>
    bool IsAssisted(int exerciseId);
}
