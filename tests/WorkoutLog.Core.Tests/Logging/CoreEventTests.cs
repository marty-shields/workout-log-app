using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Tests.Logging;

public class CoreEventTests
{
    [Test]
    public void Values_AreUnique() =>
        Assert.That(Enum.GetValues<CoreEvent>().Select(e => (int)e), Is.Unique);

    [Test]
    public void Values_AreInCoresRange() =>
        Assert.That(Enum.GetValues<CoreEvent>().Select(e => (int)e), Is.All.InRange(1000, 1999));

    [Test]
    public void Values_AreFixed() =>
        Assert.That(Enum.GetValues<CoreEvent>().ToDictionary(e => e, e => (int)e), Is.EqualTo(new Dictionary<CoreEvent, int>
        {
            [CoreEvent.SchemaVersion] = 1001,
            [CoreEvent.DatabaseCreated] = 1002,
            [CoreEvent.SchemaStepApplied] = 1003,
            [CoreEvent.ValidationRulesApplied] = 1004,
            [CoreEvent.ValidationRuleBroken] = 1005,
            [CoreEvent.ExercisesSeeded] = 1101,
            [CoreEvent.WorkoutAdded] = 1201,
            [CoreEvent.WorkoutUpdated] = 1202,
            [CoreEvent.WorkoutAddFailed] = 1203,
            [CoreEvent.WorkoutUpdateFailed] = 1204,
            [CoreEvent.WorkoutStarted] = 1206,
            [CoreEvent.WorkoutNotFound] = 1207,
            [CoreEvent.WorkoutLoadFailed] = 1208,
            [CoreEvent.WorkoutLoaded] = 1209,
            [CoreEvent.WorkoutDeleted] = 1210,
            [CoreEvent.WorkoutDeleteFailed] = 1211,
            [CoreEvent.WorkoutsLoaded] = 1301,
            [CoreEvent.WorkoutsLoadFailed] = 1302,
            [CoreEvent.WorkoutDaysLoaded] = 1303,
            [CoreEvent.WorkoutDaysLoadFailed] = 1304,
            [CoreEvent.LastWorkoutLoaded] = 1410,
            [CoreEvent.LastWorkoutLoadFailed] = 1411,
            [CoreEvent.LastWorkoutUnknownExercise] = 1412,
            [CoreEvent.ConsistencyLoaded] = 1420,
            [CoreEvent.ConsistencyLoadFailed] = 1421,
            [CoreEvent.MuscleSetsLoaded] = 1430,
            [CoreEvent.MuscleSetsLoadFailed] = 1431,
            [CoreEvent.MuscleSetsUnknownExercise] = 1432,
            [CoreEvent.RecentRecordsLoaded] = 1441,
            [CoreEvent.RecentRecordsLoadFailed] = 1442,
            [CoreEvent.RecentRecordsUnknownExercise] = 1443,
            [CoreEvent.RecentRecordsExerciseFailed] = 1444,
            [CoreEvent.PeriodComparisonLoaded] = 1450,
            [CoreEvent.PeriodComparisonLoadFailed] = 1451,
            [CoreEvent.PeriodSeriesLoaded] = 1452,
            [CoreEvent.PeriodSeriesLoadFailed] = 1453,
            [CoreEvent.PeriodStreaksLoaded] = 1454,
            [CoreEvent.PeriodStreaksLoadFailed] = 1455,
            [CoreEvent.PeriodLimitsLoaded] = 1456,
            [CoreEvent.PeriodLimitsLoadFailed] = 1457,
            [CoreEvent.PeriodRecordsLoaded] = 1460,
            [CoreEvent.PeriodRecordsLoadFailed] = 1461,
            [CoreEvent.PeriodRecordsUnknownExercise] = 1462,
            [CoreEvent.PeriodRecordsExerciseFailed] = 1463,
            [CoreEvent.PeriodMuscleSetsLoaded] = 1464,
            [CoreEvent.PeriodMuscleSetsLoadFailed] = 1465,
            [CoreEvent.PeriodMuscleSetsUnknownExercise] = 1466,
            [CoreEvent.PeriodMostTrainedLoaded] = 1467,
            [CoreEvent.PeriodMostTrainedLoadFailed] = 1468,
            [CoreEvent.PeriodMostTrainedUnknownExercise] = 1469,
            [CoreEvent.WorkoutSummaryLoaded] = 1530,
            [CoreEvent.WorkoutSummaryLoadFailed] = 1531,
            [CoreEvent.WorkoutSummaryUnknownExercise] = 1532,
            [CoreEvent.WorkoutSummaryExerciseFailed] = 1533,
            [CoreEvent.WeightHistoryLoadFailed] = 1501,
            [CoreEvent.LastSessionLoadFailed] = 1510,
            [CoreEvent.ExportWritten] = 1601,
            [CoreEvent.ExportTooLarge] = 1602,
            [CoreEvent.ExportFailed] = 1603,
            [CoreEvent.ExportUnknownExercise] = 1604,
            [CoreEvent.ExportCountFailed] = 1605,
            [CoreEvent.ExportLeftoversDeleted] = 1606,
            [CoreEvent.ExportLeftoversFailed] = 1607,
            [CoreEvent.ImportDone] = 1701,
            [CoreEvent.ImportWorkoutSkipped] = 1702,
            [CoreEvent.ImportFileUnreadable] = 1703,
            [CoreEvent.ImportPhoneReadFailed] = 1704,
            [CoreEvent.ImportWriteFailed] = 1705,
            [CoreEvent.ImportFileRejected] = 1706,
            [CoreEvent.LoggedExercisesLoaded] = 1801,
            [CoreEvent.LoggedExercisesLoadFailed] = 1802,
            [CoreEvent.LoggedExercisesUnknownExercise] = 1803,
            [CoreEvent.ExerciseProgressLoaded] = 1901,
            [CoreEvent.ExerciseProgressLoadFailed] = 1902,
            [CoreEvent.ExerciseProgressUnknownExercise] = 1903,
            [CoreEvent.ExerciseProgressNotSupported] = 1904,
            [CoreEvent.CardioProgressLoaded] = 1905,
            [CoreEvent.CardioProgressLoadFailed] = 1906,
            [CoreEvent.CardioProgressUnknownExercise] = 1907,
            [CoreEvent.CardioProgressNotCardio] = 1908,
        }));

    [Test]
    public void Values_RetiredAreNotReused() =>
        Assert.That(Enum.GetValues<CoreEvent>().Select(e => (int)e), Has.None.AnyOf(1205, 1401, 1402));
}
