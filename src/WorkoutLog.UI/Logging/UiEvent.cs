namespace WorkoutLog.UI.Logging;

/// <summary>
/// Event ids of the UI's log messages: 2000–4999, a block of 100 per type. Values never change once logged.
/// Retired, never reused: 2801 and 3001, focusing Try again failed on History and Edit Workout (now 3301).
/// </summary>
internal enum UiEvent
{
    /// <summary><see cref="Components.Layout.StartupGate"/>: startup finished, with its duration.</summary>
    StartupFinished = 2001,

    /// <summary><see cref="Components.Layout.StartupGate"/>: startup failed.</summary>
    StartupFailed = 2002,

    /// <summary><see cref="Components.Layout.AppErrorBoundary"/>: unhandled error in a screen.</summary>
    ScreenError = 2101,

    /// <summary><see cref="Navigation.BackButton"/>: back handled by the page.</summary>
    BackHandled = 2201,

    /// <summary><see cref="Navigation.BackButton"/>: back pressed with no handler.</summary>
    BackNotHandled = 2202,

    /// <summary><see cref="Navigation.BackButton"/>: a back handler failed.</summary>
    BackHandlerFailed = 2203,

    /// <summary><see cref="Screens.WorkoutView"/>: focusing an exercise failed.</summary>
    ExerciseFocusFailed = 2301,

    /// <summary><see cref="Screens.WorkoutView"/>: focusing the first error failed.</summary>
    ErrorFocusFailed = 2302,

    /// <summary><see cref="Components.Workout.ExerciseCard"/>: focusing a blocked set failed.</summary>
    BlockedSetFocusFailed = 2401,

    /// <summary><see cref="Models.Workout.SetChecker"/>: a set check failed with an exercise code.</summary>
    SetCheckExerciseError = 2501,

    /// <summary><see cref="Models.Workout.SetChecker"/>: a field code had no message.</summary>
    SetCheckFieldUnworded = 2502,

    /// <summary><see cref="Models.Workout.WorkoutChecker"/>: Save found an unknown exercise or wrong type.</summary>
    SaveCheckExerciseError = 2601,

    /// <summary><see cref="Screens.NewWorkoutPage"/>: new workout discarded.</summary>
    NewWorkoutDiscarded = 2701,

    /// <summary><see cref="Screens.EditWorkoutPage"/>: a saved exercise is unknown or the wrong type.</summary>
    EditWorkoutOpenFailed = 2901,

    /// <summary><see cref="Screens.EditWorkoutPage"/>: edits discarded.</summary>
    EditsDiscarded = 2902,

    /// <summary><see cref="Lifecycle.AppLifecycle"/>: an app start handler failed.</summary>
    AppStartHandlerFailed = 3201,

    /// <summary><see cref="Components.Feedback.LoadFailed"/>: focusing Try again failed.</summary>
    LoadFailedFocusFailed = 3301,

    /// <summary><see cref="Screens.SettingsPage"/>: opening the share sheet failed.</summary>
    ShareFailed = 3401,

    /// <summary><see cref="Screens.SettingsView"/>: focusing Export failed.</summary>
    ExportFocusFailed = 3501,
}
