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
            [CoreEvent.ExercisesSeeded] = 1101,
            [CoreEvent.WorkoutAdded] = 1201,
            [CoreEvent.WorkoutUpdated] = 1202,
            [CoreEvent.WorkoutAddFailed] = 1203,
            [CoreEvent.WorkoutUpdateFailed] = 1204,
            [CoreEvent.ClockBeforeStart] = 1205,
            [CoreEvent.WorkoutStarted] = 1206,
            [CoreEvent.WorkoutNotFound] = 1207,
            [CoreEvent.WorkoutLoadFailed] = 1208,
            [CoreEvent.WorkoutLoaded] = 1209,
            [CoreEvent.HistoryLoaded] = 1301,
            [CoreEvent.HistoryLoadFailed] = 1302,
        }));
}
