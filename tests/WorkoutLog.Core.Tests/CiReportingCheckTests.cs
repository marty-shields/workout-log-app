namespace WorkoutLog.Core.Tests;

// Temporary: fails on purpose to check the PR build's test reporting. Reverted before merge.
public class CiReportingCheckTests
{
    [Test]
    public void FailsOnPurpose() => Assert.That(1 + 1, Is.EqualTo(3));
}
