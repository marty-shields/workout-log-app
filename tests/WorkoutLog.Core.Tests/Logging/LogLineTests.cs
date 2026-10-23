using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Tests.Logging;

public class LogLineTests
{
    [TestCase(LogLevel.Trace, "TRCE")]
    [TestCase(LogLevel.Debug, "DBUG")]
    [TestCase(LogLevel.Information, "INFO")]
    [TestCase(LogLevel.Warning, "WARN")]
    [TestCase(LogLevel.Error, "FAIL")]
    [TestCase(LogLevel.Critical, "CRIT")]
    public void Format_NoException_IsLevelCategoryEventIdAndMessage(LogLevel level, string expected) =>
        Assert.That(LogLine.Format(level, "WorkoutLog.Core.Data.ExerciseSeeder", new EventId(1101), "Seeded", null),
            Is.EqualTo($"{expected} WorkoutLog.Core.Data.ExerciseSeeder[1101] Seeded"));

    [Test]
    public void Format_WithException_PutsTheExceptionOnTheNextLines()
    {
        var exception = new InvalidOperationException("Broken");

        Assert.That(LogLine.Format(LogLevel.Error, "Category", new EventId(1203), "Failed", exception),
            Is.EqualTo($"FAIL Category[1203] Failed{Environment.NewLine}{exception}"));
    }
}
