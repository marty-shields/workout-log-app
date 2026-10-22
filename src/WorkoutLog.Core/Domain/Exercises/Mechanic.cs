namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>Movement mechanic of a strength exercise.</summary>
public enum Mechanic
{
    /// <summary>Works more than one joint.</summary>
    Compound,

    /// <summary>Works one joint.</summary>
    Isolation,
}
