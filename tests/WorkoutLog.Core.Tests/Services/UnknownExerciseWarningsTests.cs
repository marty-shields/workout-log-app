using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;

namespace WorkoutLog.Core.Tests.Services;

public class UnknownExerciseWarningsTests
{
    [Test]
    public void IsFirst_OnlyTheFirstReportOfAnIdUnderAnEvent_IsTrue()
    {
        var warnings = new UnknownExerciseWarnings();

        Assert.Multiple(() =>
        {
            Assert.That(warnings.IsFirst(CoreEvent.PeriodRecordsUnknownExercise, 9), Is.True);
            Assert.That(warnings.IsFirst(CoreEvent.PeriodRecordsUnknownExercise, 9), Is.False);
            Assert.That(warnings.IsFirst(CoreEvent.PeriodRecordsUnknownExercise, 10), Is.True);
            Assert.That(warnings.IsFirst(CoreEvent.PeriodVolumeUnknownExercise, 9), Is.True);
        });
    }

    [Test]
    public void IsFirst_ManyThreadsReportTheSameId_IsTrueForExactlyOne()
    {
        var warnings = new UnknownExerciseWarnings();
        var firsts = 0;

        Parallel.For(0, 1000, _ =>
        {
            if (warnings.IsFirst(CoreEvent.PeriodVolumeUnknownExercise, 9))
            {
                Interlocked.Increment(ref firsts);
            }
        });

        Assert.That(firsts, Is.EqualTo(1));
    }
}
