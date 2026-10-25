namespace WorkoutLog.Core.Tests;

// Temporary: fails on purpose to check the PR build's test reporting. Reverted before merge.
public class CiReportingCheckTests
{
    [Test]
    public void FailsOnPurpose()
    {
        var actual = new[] { 1, 1 }.Sum();

        Assert.That(actual, Is.EqualTo(3));
    }
}
