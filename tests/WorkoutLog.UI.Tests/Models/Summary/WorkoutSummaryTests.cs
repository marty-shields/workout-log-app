using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Models.Summary;

public class WorkoutSummaryTests
{
    [Test]
    public void IsFirstWorkout_WhenThereAreNoChanges()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SampleWorkoutSummary.FirstWorkout.IsFirstWorkout, Is.True);
            Assert.That(SampleWorkoutSummary.AllFirstTime.IsFirstWorkout, Is.False);
            Assert.That(SampleWorkoutSummary.RecordsAndChanges.IsFirstWorkout, Is.False);
        });
    }
}
