namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>Most exercises a workout and sets a strength exercise accept.</summary>
public static class WorkoutLimits
{
    /// <summary>Most exercises in a workout.</summary>
    public const int MaxExercises = 10;

    /// <summary>Most sets of a strength exercise.</summary>
    public const int MaxSets = 10;
}
