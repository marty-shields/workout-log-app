namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>Unit of an exercise's rate.</summary>
public enum RateUnit
{
    /// <summary>Kilometres per hour.</summary>
    KmPerHour,

    /// <summary>Pace in minutes per kilometre.</summary>
    MinPerKm,

    /// <summary>Revolutions per minute.</summary>
    Rpm,

    /// <summary>Strokes per minute.</summary>
    StrokesPerMin,

    /// <summary>Strides per minute.</summary>
    StridesPerMin,

    /// <summary>Steps per minute.</summary>
    StepsPerMin,

    /// <summary>Feet per minute.</summary>
    FeetPerMin,
}
