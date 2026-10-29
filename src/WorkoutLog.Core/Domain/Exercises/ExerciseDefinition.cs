namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>An exercise in the fixed list.</summary>
/// <param name="Id">Exercise id. Never reused or changed.</param>
/// <param name="Name">Unique name.</param>
/// <param name="Type">Strength or cardio.</param>
/// <param name="Category">How its sets are measured: lift, bodyweight, assisted, hold, carry or cardio.</param>
/// <param name="Aliases">Nicknames for search. Unique across the list.</param>
/// <param name="PrimaryMuscle">Strength only.</param>
/// <param name="Equipment">Strength only.</param>
/// <param name="Mechanic">Strength only.</param>
/// <param name="Metrics">Metrics shown, in field order.</param>
/// <param name="DistanceUnit">Unit of the distance metric. Null when there is no distance.</param>
/// <param name="RateUnit">Unit of the rate metric; <see cref="Exercises.RateUnit.KmPerHour"/> marks worked-out speed (<see cref="WorkedOut.HasSpeed"/>). Null otherwise.</param>
/// <param name="PaceUnit">Unit of the worked-out pace (<see cref="WorkedOut.HasPace"/>). Null when there is no pace.</param>
/// <param name="Loaded">True for a bodyweight move whose Exercise page has an e1RM tab (D193).</param>
public sealed record ExerciseDefinition(
    int Id,
    string Name,
    ExerciseType Type,
    ExerciseCategory Category,
    IReadOnlyList<string> Aliases,
    Muscle? PrimaryMuscle,
    Equipment? Equipment,
    Mechanic? Mechanic,
    IReadOnlyList<MetricDefinition> Metrics,
    DistanceUnit? DistanceUnit,
    RateUnit? RateUnit,
    PaceUnit? PaceUnit,
    bool Loaded)
{
    /// <summary>True for an assisted machine (<see cref="ExerciseCategory.Assisted"/>): its weight is the assistance.</summary>
    public bool IsAssisted => Category == ExerciseCategory.Assisted;
}
