namespace WorkoutLog.UI.Models.Workout;

/// <summary>The value a cardio entry shows worked out from its distance and duration.</summary>
public enum WorkedOutKind
{
    None,

    /// <summary>Pace, time per km.</summary>
    Pace,

    /// <summary>Speed, km/h.</summary>
    Speed,
}
