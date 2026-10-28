namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>How an exercise's sets are measured, which decides how its best is picked.</summary>
public enum ExerciseCategory
{
    /// <summary>Strength: reps with weight.</summary>
    Lift,

    /// <summary>Strength: reps, with weight as optional extra load.</summary>
    Bodyweight,

    /// <summary>Strength: reps on an assisted machine; the weight is the assistance.</summary>
    Assisted,

    /// <summary>Strength: a held duration, with optional weight.</summary>
    Hold,

    /// <summary>Strength: a distance carried, with optional weight and duration.</summary>
    Carry,

    /// <summary>Cardio: one entry.</summary>
    Cardio,
}
