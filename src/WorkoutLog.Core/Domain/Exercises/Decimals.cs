namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>Rounding for decimal metrics (D29, D47).</summary>
public static class Decimals
{
    /// <summary>Rounds to 2 decimal places, halves away from zero.</summary>
    public static decimal Round2(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
