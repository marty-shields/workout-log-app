namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>Pace and speed, worked out from a cardio entry's distance and duration; never stored.</summary>
public static class WorkedOut
{
    /// <summary>True when the exercise shows pace: it has a pace unit.</summary>
    public static bool HasPace(ExerciseDefinition exercise) => exercise.PaceUnit is not null;

    /// <summary>True when the exercise shows speed: its rate unit is km/h.</summary>
    public static bool HasSpeed(ExerciseDefinition exercise) => exercise.RateUnit == RateUnit.KmPerHour;

    /// <summary>Time per km, unrounded. Null unless both values are more than 0.</summary>
    public static TimeSpan? Pace(TimeSpan? duration, decimal? distanceKm) =>
        (duration, distanceKm) is ({ Ticks: > 0 } time, { } km) && km > 0 ? TimeSpan.FromTicks((long)(time.Ticks / km)) : null;

    /// <summary>Km per hour, unrounded. Null unless both values are more than 0.</summary>
    public static decimal? Speed(TimeSpan? duration, decimal? distanceKm) =>
        (duration, distanceKm) is ({ Ticks: > 0 } time, { } km) && km > 0 ? km * TimeSpan.TicksPerHour / time.Ticks : null;

    /// <summary>Pace for display: to the second, halves away from zero.</summary>
    public static TimeSpan RoundPace(TimeSpan pace) =>
        TimeSpan.FromSeconds((double)Math.Round((decimal)pace.Ticks / TimeSpan.TicksPerSecond, MidpointRounding.AwayFromZero));

    /// <summary>Speed for display: to 2 decimal places, halves away from zero.</summary>
    public static decimal RoundSpeed(decimal speed) => Decimals.Round2(speed);
}
