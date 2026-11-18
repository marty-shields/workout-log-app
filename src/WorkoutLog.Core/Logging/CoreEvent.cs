namespace WorkoutLog.Core.Logging;

/// <summary>
/// Event ids of Core's log messages: 1000–1999, a block of 100 per type; the This week and Trends services share 1400 and the Exercise page services share 1900.
/// Ids never change and are never reused. Retired: 1205, 1401, 1402, 1410-1412, 1420, 1421, 1430-1432, 1441-1444.
/// </summary>
internal enum CoreEvent
{
    /// <summary><see cref="Data.DatabaseInitializer"/>: stored and current schema versions.</summary>
    SchemaVersion = 1001,

    /// <summary><see cref="Data.DatabaseInitializer"/>: new database created.</summary>
    DatabaseCreated = 1002,

    /// <summary><see cref="Data.DatabaseInitializer"/>: a schema upgrade step ran.</summary>
    SchemaStepApplied = 1003,

    /// <summary><see cref="Data.ValidationRulesSchema"/>: schema step 4's changes, as counts.</summary>
    ValidationRulesApplied = 1004,

    /// <summary><see cref="Data.ValidationRulesSchema"/>: rows break a rule; schema step 4 fails.</summary>
    ValidationRuleBroken = 1005,

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

    /// <summary><see cref="Services.WorkoutsService"/>: the workouts per local day read.</summary>
    WorkoutDaysLoaded = 1303,

    /// <summary><see cref="Services.WorkoutsService"/>: reading the workouts per local day failed.</summary>
    WorkoutDaysLoadFailed = 1304,

    /// <summary><see cref="Services.PeriodTotalsService"/>: a period's comparison read.</summary>
    PeriodComparisonLoaded = 1450,

    /// <summary><see cref="Services.PeriodTotalsService"/>: reading a period's comparison failed.</summary>
    PeriodComparisonLoadFailed = 1451,

    /// <summary><see cref="Services.PeriodTotalsService"/>: a period's series read.</summary>
    PeriodSeriesLoaded = 1452,

    /// <summary><see cref="Services.PeriodTotalsService"/>: reading a period's series failed.</summary>
    PeriodSeriesLoadFailed = 1453,

    /// <summary><see cref="Services.PeriodTotalsService"/>: the streaks read.</summary>
    PeriodStreaksLoaded = 1454,

    /// <summary><see cref="Services.PeriodTotalsService"/>: reading the streaks failed.</summary>
    PeriodStreaksLoadFailed = 1455,

    /// <summary><see cref="Services.PeriodTotalsService"/>: the period limits read.</summary>
    PeriodLimitsLoaded = 1456,

    /// <summary><see cref="Services.PeriodTotalsService"/>: reading the period limits failed.</summary>
    PeriodLimitsLoadFailed = 1457,

    /// <summary><see cref="Services.PeriodTotalsService"/>: reading a period's volume failed.</summary>
    PeriodVolumeLoadFailed = 1458,

    /// <summary><see cref="Services.PeriodTotalsService"/>: a saved exercise id is not in the exercise list.</summary>
    PeriodVolumeUnknownExercise = 1459,

    /// <summary><see cref="Services.PeriodTrainingService"/>: a period's records read.</summary>
    PeriodRecordsLoaded = 1460,

    /// <summary><see cref="Services.PeriodTrainingService"/>: reading a period's records failed.</summary>
    PeriodRecordsLoadFailed = 1461,

    /// <summary><see cref="Services.PeriodTrainingService"/>: a saved exercise id is not in the exercise list.</summary>
    PeriodRecordsUnknownExercise = 1462,

    /// <summary><see cref="Services.PeriodTrainingService"/>: finding one exercise's records failed.</summary>
    PeriodRecordsExerciseFailed = 1463,

    /// <summary><see cref="Services.PeriodTrainingService"/>: a period's sets per muscle group read.</summary>
    PeriodMuscleSetsLoaded = 1464,

    /// <summary><see cref="Services.PeriodTrainingService"/>: reading a period's sets per muscle group failed.</summary>
    PeriodMuscleSetsLoadFailed = 1465,

    /// <summary><see cref="Services.PeriodTrainingService"/>: a saved exercise id is not in the exercise list.</summary>
    PeriodMuscleSetsUnknownExercise = 1466,

    /// <summary><see cref="Services.PeriodTrainingService"/>: a period's most trained exercises read.</summary>
    PeriodMostTrainedLoaded = 1467,

    /// <summary><see cref="Services.PeriodTrainingService"/>: reading a period's most trained exercises failed.</summary>
    PeriodMostTrainedLoadFailed = 1468,

    /// <summary><see cref="Services.PeriodTrainingService"/>: a saved exercise id is not in the exercise list.</summary>
    PeriodMostTrainedUnknownExercise = 1469,

    /// <summary><see cref="Services.PeriodTrainingService"/>: a period's workouts read.</summary>
    PeriodWorkoutsLoaded = 1470,

    /// <summary><see cref="Services.PeriodTrainingService"/>: reading a period's workouts failed.</summary>
    PeriodWorkoutsLoadFailed = 1471,

    /// <summary><see cref="Services.PeriodTrainingService"/>: a saved exercise id is not in the exercise list.</summary>
    PeriodWorkoutsUnknownExercise = 1472,

    /// <summary><see cref="Services.WorkoutSummaryService"/>: a workout's summary read.</summary>
    WorkoutSummaryLoaded = 1530,

    /// <summary><see cref="Services.WorkoutSummaryService"/>: reading a workout's summary failed.</summary>
    WorkoutSummaryLoadFailed = 1531,

    /// <summary><see cref="Services.WorkoutSummaryService"/>: a saved exercise id is not in the exercise list.</summary>
    WorkoutSummaryUnknownExercise = 1532,

    /// <summary><see cref="Services.WorkoutSummaryService"/>: summarising an exercise failed.</summary>
    WorkoutSummaryExerciseFailed = 1533,

    /// <summary><see cref="Services.WeightHistoryService"/>: reading an exercise's weights failed.</summary>
    WeightHistoryLoadFailed = 1501,

    /// <summary><see cref="Services.LastSessionService"/>: reading an exercise's last session failed.</summary>
    LastSessionLoadFailed = 1510,

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

    /// <summary><see cref="Services.ExportService"/>: earlier export files deleted.</summary>
    ExportLeftoversDeleted = 1606,

    /// <summary><see cref="Services.ExportService"/>: deleting earlier export files failed.</summary>
    ExportLeftoversFailed = 1607,

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

    /// <summary><see cref="Services.ExerciseProgressService"/>: an exercise's sessions read.</summary>
    ExerciseProgressLoaded = 1901,

    /// <summary><see cref="Services.ExerciseProgressService"/>: reading an exercise's sessions failed.</summary>
    ExerciseProgressLoadFailed = 1902,

    /// <summary><see cref="Services.ExerciseProgressService"/>: the exercise id is not in the exercise list.</summary>
    ExerciseProgressUnknownExercise = 1903,

    /// <summary><see cref="Services.ExerciseProgressService"/>: the exercise has no strength progress page.</summary>
    ExerciseProgressNotSupported = 1904,

    /// <summary><see cref="Services.CardioProgressService"/>: a cardio exercise's sessions read.</summary>
    CardioProgressLoaded = 1905,

    /// <summary><see cref="Services.CardioProgressService"/>: reading a cardio exercise's sessions failed.</summary>
    CardioProgressLoadFailed = 1906,

    /// <summary><see cref="Services.CardioProgressService"/>: the exercise id is not in the exercise list.</summary>
    CardioProgressUnknownExercise = 1907,

    /// <summary><see cref="Services.CardioProgressService"/>: the exercise is not cardio.</summary>
    CardioProgressNotCardio = 1908,
}
