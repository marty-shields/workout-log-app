using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Bests;

/// <summary>Exercises and sessions for the best rule tests.</summary>
internal static class BestRuleCases
{
    /// <summary>Barbell Back Squat: lift.</summary>
    public const int Squat = 6;

    /// <summary>Dumbbell Curl: lift.</summary>
    public const int Curl = 132;

    /// <summary>Push-up: bodyweight, equipment None.</summary>
    public const int PushUp = 264;

    /// <summary>Band Pull-Apart: bodyweight, equipment Band.</summary>
    public const int BandPullApart = 4;

    /// <summary>Plate Front Raise: lift, equipment Other.</summary>
    public const int PlateFrontRaise = 259;

    /// <summary>Farmer's Walk: carry, equipment Other.</summary>
    public const int FarmersWalk = 187;

    /// <summary>Machine Assisted Pull-up: assisted.</summary>
    public const int AssistedPullUp = 218;

    /// <summary>Plank: hold.</summary>
    public const int Plank = 258;

    /// <summary>Outdoor Run: cardio with distance.</summary>
    public const int Run = 311;

    /// <summary>Jump Rope: cardio without distance.</summary>
    public const int JumpRope = 315;

    public static readonly ExerciseCatalog Catalog = new();

    public static ExerciseDefinition Definition(int id) => Catalog.All.Single(e => e.Id == id);

    /// <summary>One strength exercise with the sets.</summary>
    public static WorkoutExercise[] Sets(int id, params StrengthSet[] sets) => [new StrengthExercise(id, sets)];

    public static StrengthSet Set(int? reps, decimal? weightKg, TimeSpan? duration = null, decimal? distance = null) =>
        new(reps, weightKg, duration, distance);

    public static CardioExercise Entry(int id, TimeSpan? duration, decimal? distance) =>
        new(id, new CardioEntry(duration, distance, null, null, null, null, null, null));
}
