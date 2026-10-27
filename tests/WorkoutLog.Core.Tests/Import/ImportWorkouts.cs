using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Import;
using static WorkoutLog.Core.Tests.Import.ImportJson;

namespace WorkoutLog.Core.Tests.Import;

/// <summary>Import workouts for check tests.</summary>
internal static class ImportWorkouts
{
    /// <summary>A Barbell Bench Press (8) with one set of 8 × 60 kg, typed as in the exercise list.</summary>
    public static readonly ImportExercise Bench = new(new ImportExerciseInfo(8, "Barbell Bench Press", ExerciseType.Strength), [new ImportSet(8, 60m)]);

    /// <summary>An Outdoor Run (311) of 30 minutes, typed as in the exercise list.</summary>
    public static readonly ImportExercise Run = new(
        new ImportExerciseInfo(311, "Outdoor Run", ExerciseType.Cardio, DistanceUnit.Km, PaceUnit: PaceUnit.PerKm),
        Entry: new ImportEntry(TimeSpan.FromMinutes(30), 5.2m));

    /// <summary>A workout at <paramref name="hours"/> after <see cref="ImportJson.Start"/>, an hour long, with <paramref name="exercises"/> (Bench when none).</summary>
    public static ImportWorkout At(int hours, params ImportExercise[] exercises) =>
        new(1, Time(Start.AddHours(hours)), Time(Start.AddHours(hours + 1)), exercises.Length == 0 ? [Bench] : exercises);
}
