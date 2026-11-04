namespace WorkoutLog.UI.Haptics;

/// <summary>
/// A moment in the logging loop worth feeling with the phone on a bench. Played only where the action succeeded or was blocked,
/// never on navigation, deletes, undo or typing.
/// </summary>
public enum HapticCue
{
    /// <summary>Add set closed the open set as valid and opened a new one.</summary>
    SetLogged,

    /// <summary>An exercise was marked done.</summary>
    ExerciseDone,

    /// <summary>A new or edited workout was saved.</summary>
    WorkoutSaved,

    /// <summary>Add set, Done or Save was blocked by validation errors.</summary>
    Rejected,
}
