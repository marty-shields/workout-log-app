namespace WorkoutLog.Core.Logging;

/// <summary>Event ids of Core's log messages: 1000–1999, a block of 100 per type. Values never change once logged.</summary>
internal enum CoreEvent
{
    /// <summary><see cref="Data.DatabaseInitializer"/>: stored and current schema versions.</summary>
    SchemaVersion = 1001,

    /// <summary><see cref="Data.DatabaseInitializer"/>: new database created.</summary>
    DatabaseCreated = 1002,

    /// <summary><see cref="Data.DatabaseInitializer"/>: a schema upgrade step ran.</summary>
    SchemaStepApplied = 1003,

    /// <summary><see cref="Data.ExerciseSeeder"/>: exercises added and updated.</summary>
    ExercisesSeeded = 1101,

    /// <summary><see cref="Services.WorkoutService"/>: new workout saved.</summary>
    WorkoutAdded = 1201,

    /// <summary><see cref="Services.WorkoutService"/>: edited workout saved.</summary>
    WorkoutUpdated = 1202,

    /// <summary><see cref="Services.WorkoutService"/>: saving a new workout failed.</summary>
    WorkoutAddFailed = 1203,

    /// <summary><see cref="Services.WorkoutService"/>: saving an edited workout failed.</summary>
    WorkoutUpdateFailed = 1204,

    /// <summary><see cref="Services.WorkoutService"/>: the clock is before the workout start on first save.</summary>
    ClockBeforeStart = 1205,

    /// <summary><see cref="Services.WorkoutService"/>: new workout started.</summary>
    WorkoutStarted = 1206,

    /// <summary><see cref="Services.WorkoutService"/>: no workout has the id.</summary>
    WorkoutNotFound = 1207,

    /// <summary><see cref="Services.WorkoutService"/>: reading a workout failed.</summary>
    WorkoutLoadFailed = 1208,

    /// <summary><see cref="Services.WorkoutService"/>: workout read.</summary>
    WorkoutLoaded = 1209,

    /// <summary><see cref="Services.WorkoutService"/>: workout deleted.</summary>
    WorkoutDeleted = 1210,

    /// <summary><see cref="Services.WorkoutService"/>: deleting a workout failed.</summary>
    WorkoutDeleteFailed = 1211,

    /// <summary><see cref="Services.WorkoutsService"/>: saved workouts read.</summary>
    WorkoutsLoaded = 1301,

    /// <summary><see cref="Services.WorkoutsService"/>: reading the saved workouts failed.</summary>
    WorkoutsLoadFailed = 1302,

    /// <summary><see cref="Services.DashboardService"/>: Dashboard read.</summary>
    DashboardLoaded = 1401,

    /// <summary><see cref="Services.DashboardService"/>: reading the Dashboard failed.</summary>
    DashboardLoadFailed = 1402,

    /// <summary><see cref="Services.WeightHistoryService"/>: reading an exercise's weights failed.</summary>
    WeightHistoryLoadFailed = 1501,

    /// <summary><see cref="Services.ExportService"/>: export file written.</summary>
    ExportWritten = 1601,

    /// <summary><see cref="Services.ExportService"/>: export file over the size limit; not written.</summary>
    ExportTooLarge = 1602,

    /// <summary><see cref="Services.ExportService"/>: exporting failed.</summary>
    ExportFailed = 1603,

    /// <summary><see cref="Services.ExportService"/>: a saved exercise id is not in the exercise list.</summary>
    ExportUnknownExercise = 1604,

    /// <summary><see cref="Services.ExportService"/>: counting the workouts failed.</summary>
    ExportCountFailed = 1605,

    /// <summary><see cref="Services.ImportService"/>: import written.</summary>
    ImportDone = 1701,

    /// <summary><see cref="Services.ImportService"/>: a workout in the file can't be imported.</summary>
    ImportWorkoutSkipped = 1702,

    /// <summary><see cref="Services.ImportService"/>: reading or parsing the file threw.</summary>
    ImportFileUnreadable = 1703,

    /// <summary><see cref="Services.ImportService"/>: reading the phone's workouts failed.</summary>
    ImportPhoneReadFailed = 1704,

    /// <summary><see cref="Services.ImportService"/>: writing the import failed.</summary>
    ImportWriteFailed = 1705,

    /// <summary><see cref="Services.ImportService"/>: the file isn't an export, is newer, has no workouts or is too large.</summary>
    ImportFileRejected = 1706,

    /// <summary><see cref="Services.LoggedExercisesService"/>: logged exercises read.</summary>
    LoggedExercisesLoaded = 1801,

    /// <summary><see cref="Services.LoggedExercisesService"/>: reading the logged exercises failed.</summary>
    LoggedExercisesLoadFailed = 1802,

    /// <summary><see cref="Services.LoggedExercisesService"/>: a saved exercise id is not in the exercise list.</summary>
    LoggedExercisesUnknownExercise = 1803,

    /// <summary><see cref="Services.ExerciseProgressService"/>: a lift's sessions read.</summary>
    ExerciseProgressLoaded = 1901,

    /// <summary><see cref="Services.ExerciseProgressService"/>: reading a lift's sessions failed.</summary>
    ExerciseProgressLoadFailed = 1902,

    /// <summary><see cref="Services.ExerciseProgressService"/>: the exercise id is not in the exercise list.</summary>
    ExerciseProgressUnknownExercise = 1903,

    /// <summary><see cref="Services.ExerciseProgressService"/>: the exercise is not a lift.</summary>
    ExerciseProgressNotALift = 1904,
}
