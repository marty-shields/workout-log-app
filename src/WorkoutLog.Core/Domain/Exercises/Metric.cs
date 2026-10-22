namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>A value recorded for a strength set or cardio entry.</summary>
public enum Metric
{
    /// <summary>Repetitions. Strength.</summary>
    Reps,

    /// <summary>Weight in kg. Strength.</summary>
    WeightKg,

    /// <summary>Duration. Strength and cardio.</summary>
    Duration,

    /// <summary>Distance, in the exercise's distance unit. Strength and cardio.</summary>
    Distance,

    /// <summary>Floors climbed. Cardio.</summary>
    Floors,

    /// <summary>Resistance, damper or speed level. Cardio.</summary>
    Level,

    /// <summary>Incline in percent. Cardio.</summary>
    Incline,

    /// <summary>Calories. Cardio.</summary>
    Calories,

    /// <summary>Rate, in the exercise's rate unit. Cardio.</summary>
    Rate,
}
