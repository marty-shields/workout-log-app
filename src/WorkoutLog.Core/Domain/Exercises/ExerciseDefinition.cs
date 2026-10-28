namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>An exercise in the fixed list.</summary>
/// <param name="Id">Exercise id. Never reused or changed.</param>
/// <param name="Name">Unique name.</param>
/// <param name="Type">Strength or cardio.</param>
/// <param name="Aliases">Nicknames for search. Unique across the list.</param>
/// <param name="PrimaryMuscle">Strength only.</param>
/// <param name="Equipment">Strength only.</param>
/// <param name="Mechanic">Strength only.</param>
/// <param name="Metrics">Metrics shown, in field order.</param>
/// <param name="DistanceUnit">Unit of the distance metric. Null when there is no distance.</param>
/// <param name="RateUnit">Unit of the rate metric. Null when there is no rate.</param>
/// <param name="PaceUnit">Unit of the pace metric. Null when there is no pace.</param>
public sealed record ExerciseDefinition(
    int Id,
    string Name,
    ExerciseType Type,
    IReadOnlyList<string> Aliases,
    Muscle? PrimaryMuscle,
    Equipment? Equipment,
    Mechanic? Mechanic,
    IReadOnlyList<MetricDefinition> Metrics,
    DistanceUnit? DistanceUnit,
    RateUnit? RateUnit,
    PaceUnit? PaceUnit)
{
    /// <summary>Weight label of the assisted machines.</summary>
    private const string AssistLabel = "Assist";

    /// <summary>True for an assisted machine: its weight is labelled <see cref="AssistLabel"/> and is the assistance.</summary>
    public bool IsAssisted => Metrics.Any(m => m is { Metric: Metric.WeightKg, Label: AssistLabel });
}
