namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>Most exercises a workout, sets a strength exercise and characters a note accept.</summary>
public static class WorkoutLimits
{
    /// <summary>Most exercises in a workout.</summary>
    public const int MaxExercises = 10;

    /// <summary>Most sets of a strength exercise.</summary>
    public const int MaxSets = 10;

    /// <summary>Most characters in a workout note or an exercise note.</summary>
    public const int MaxNoteLength = 500;
}
