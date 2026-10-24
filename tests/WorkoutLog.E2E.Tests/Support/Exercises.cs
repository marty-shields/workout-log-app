using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.E2E.Tests.Support;

/// <summary>The app's exercise list, from Core, looked up by the names steps use.</summary>
internal static class Exercises
{
    private static readonly Lazy<IExerciseCatalog> Catalog = new(() => new ExerciseCatalog());

    public static IReadOnlyList<ExerciseDefinition> All => Catalog.Value.All;

    /// <summary>The exercise with this exact name.</summary>
    public static ExerciseDefinition Named(string name) =>
        All.FirstOrDefault(e => e.Name == name) ?? throw new ArgumentException($"No exercise is named \"{name}\".", nameof(name));

    /// <summary>The field key the app uses in test ids for a metric, e.g. weight.</summary>
    public static string Key(Metric metric) => metric switch
    {
        Metric.WeightKg => "weight",
        _ => metric.ToString().ToLowerInvariant(),
    };

    /// <summary>Field keys of the exercise's sets or entry, in display order.</summary>
    public static IReadOnlyList<string> Keys(ExerciseDefinition exercise) => exercise.Metrics.Select(m => Key(m.Metric)).ToList();
}
