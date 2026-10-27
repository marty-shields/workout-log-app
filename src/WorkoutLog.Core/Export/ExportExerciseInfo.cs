using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Export;

/// <summary>An exercise's id, with its name, type and units from the exercise list. Only the id when the list doesn't have it.</summary>
/// <param name="Id">Exercise id.</param>
/// <param name="Name">Name.</param>
/// <param name="Type">Strength or cardio.</param>
/// <param name="DistanceUnit">Unit of the distance metric, when it has one.</param>
/// <param name="RateUnit">Unit of the rate metric, when it has one.</param>
/// <param name="PaceUnit">Unit of the pace metric, when it has one.</param>
internal sealed record ExportExerciseInfo(
    int Id,
    string? Name = null,
    ExerciseType? Type = null,
    DistanceUnit? DistanceUnit = null,
    RateUnit? RateUnit = null,
    PaceUnit? PaceUnit = null);
