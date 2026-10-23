using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Tests;

/// <summary>Reads what a <see cref="FakeLogger"/> recorded.</summary>
internal static class LoggedEvents
{
    /// <summary>Event id, level and message of each record, in order.</summary>
    public static List<(UiEvent Id, LogLevel Level, string Message)> Of(FakeLogger logger) =>
        logger.Collector.GetSnapshot().Select(r => ((UiEvent)r.Id.Id, r.Level, r.Message)).ToList();
}
