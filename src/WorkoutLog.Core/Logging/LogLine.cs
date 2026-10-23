using Microsoft.Extensions.Logging;

namespace WorkoutLog.Core.Logging;

/// <summary>Formats a log entry as text.</summary>
public static class LogLine
{
    /// <summary>"Level Category[EventId] Message", then the exception on the next lines.</summary>
    /// <param name="level">Entry level.</param>
    /// <param name="category">Logger category.</param>
    /// <param name="eventId">Entry event id.</param>
    /// <param name="message">Formatted message.</param>
    /// <param name="exception">Exception, if any.</param>
    public static string Format(LogLevel level, string category, EventId eventId, string message, Exception? exception)
    {
        var line = $"{Level(level)} {category}[{eventId.Id}] {message}";
        return exception is null ? line : $"{line}{Environment.NewLine}{exception}";
    }

    private static string Level(LogLevel level) => level switch
    {
        LogLevel.Trace => "TRCE",
        LogLevel.Debug => "DBUG",
        LogLevel.Information => "INFO",
        LogLevel.Warning => "WARN",
        LogLevel.Error => "FAIL",
        LogLevel.Critical => "CRIT",
        _ => "NONE",
    };
}
