namespace WorkoutLog.UI.Tests;

// Temporary: fails on purpose to check the PR build's test reporting. Reverted before merge.
public class CiReportingCheckTests
{
    [Test]
    public void FailsOnPurpose() => Assert.That("ui", Is.EqualTo("UI"));
}
