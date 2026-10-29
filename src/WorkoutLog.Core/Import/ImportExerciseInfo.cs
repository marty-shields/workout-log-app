using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Import;

/// <summary>An exercise's id, with the name, type and units the file gives. Values left out are null.</summary>
/// <param name="Id">Exercise id. Required.</param>
/// <param name="Name">Name. Not checked.</param>
/// <param name="Type">Strength or cardio.</param>
/// <param name="DistanceUnit">Unit of the distance metric.</param>
/// <param name="RateUnit">Unit of the rate metric or worked-out speed.</param>
/// <param name="PaceUnit">Unit of the worked-out pace.</param>
internal sealed record ImportExerciseInfo(
    int Id,
    string? Name = null,
    ExerciseType? Type = null,
    DistanceUnit? DistanceUnit = null,
    RateUnit? RateUnit = null,
    PaceUnit? PaceUnit = null);
