namespace WorkoutLog.UI.Logging;

/// <summary>
/// Event ids of the UI's log messages: 2000–4999, a block of 100 per type. Values never change once logged.
/// Retired, never reused: 2001, 2002, 2101, 2201, 2202, 2203, 2801, 3001, 3201, 3301, 3901, 3902 (the kit now logs the start-up, screen error, back button and app start events, and 3301, 3901 and 3902 as 9401, 9501 and 9502).
/// </summary>
internal enum UiEvent
{
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

    /// <summary><see cref="Models.Workout.WorkoutChecker"/>: Save found an unknown exercise, wrong type or other exercise code with no message.</summary>
    SaveCheckExerciseError = 2601,

    /// <summary><see cref="Models.Workout.WorkoutChecker"/>: Save found a workout code with no message.</summary>
    SaveCheckWorkoutError = 2602,

    /// <summary><see cref="Screens.NewWorkoutPage"/>: new workout discarded.</summary>
    NewWorkoutDiscarded = 2701,

    /// <summary><see cref="Screens.EditWorkoutPage"/>: a saved exercise is unknown or the wrong type.</summary>
    EditWorkoutOpenFailed = 2901,

    /// <summary><see cref="Screens.EditWorkoutPage"/>: edits discarded.</summary>
    EditsDiscarded = 2902,

    /// <summary><see cref="Screens.ThisWeekPage"/>: reading the week failed, so the screen shows the error.</summary>
    ThisWeekLoadFailed = 3101,

    /// <summary><see cref="Screens.ThisWeekPage"/>: reading the week's records failed, so their card is left out.</summary>
    ThisWeekRecordsFailed = 3102,

    /// <summary><see cref="Screens.SettingsPage"/>: opening the share sheet failed.</summary>
    ShareFailed = 3401,

    /// <summary><see cref="Screens.SettingsPage"/>: picking or opening an import file failed.</summary>
    SettingsPickFailed = 3402,

    /// <summary><see cref="Screens.SettingsPage"/>: reading the saved theme failed.</summary>
    ThemeLoadFailed = 3403,

    /// <summary><see cref="Screens.SettingsPage"/>: saving the chosen theme failed.</summary>
    ThemeSaveFailed = 3404,

    /// <summary><see cref="Screens.SettingsPage"/>: storage refused the chosen theme, so it is lost at the next start.</summary>
    ThemeNotSaved = 3405,

    /// <summary><see cref="Screens.SettingsView"/>: focusing Export failed.</summary>
    ExportFocusFailed = 3501,

    /// <summary><see cref="Screens.SettingsView"/>: focusing Import failed.</summary>
    SettingsImportFocusFailed = 3502,

    /// <summary><see cref="Screens.ImportReviewView"/>: focusing Import failed.</summary>
    ReviewImportFocusFailed = 3601,

    /// <summary><see cref="Screens.ImportReviewPage"/>: picking or opening another import file failed.</summary>
    ReviewPickFailed = 3701,

    /// <summary><see cref="Import.ImportFileOpener"/>: the picked file is over the size limit.</summary>
    ImportFileTooLarge = 3801,

    /// <summary><see cref="Components.Layout.AppScreen"/>: setting up pull to refresh failed, so the screen can't be pulled.</summary>
    PullRefreshFailed = 4001,

    /// <summary><see cref="Screens.TrendsPage"/>: reading the period failed, so the screen shows the error.</summary>
    TrendsLoadFailed = 4101,

    /// <summary><see cref="Screens.TrendsPage"/>: reading the period's records failed, so their card is empty.</summary>
    TrendsRecordsFailed = 4102,
}
