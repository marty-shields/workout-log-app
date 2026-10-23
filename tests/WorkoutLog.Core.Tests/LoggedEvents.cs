using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace WorkoutLog.Core.Tests;

/// <summary>Reads what a <see cref="FakeLogger"/> recorded.</summary>
public static class LoggedEvents
{
    /// <summary>Event id, level and message of each record, in order.</summary>
    public static List<(int Id, LogLevel Level, string Message)> Of(FakeLogger logger) =>
        logger.Collector.GetSnapshot().Select(r => (r.Id.Id, r.Level, r.Message)).ToList();
}
