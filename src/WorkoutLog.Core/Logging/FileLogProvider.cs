using System.Globalization;
using Microsoft.Extensions.Logging;

namespace WorkoutLog.Core.Logging;

/// <summary>Writes log entries through <see cref="FileLogWriter"/>, each line starting with the UTC time.</summary>
/// <param name="writer">Writes the lines.</param>
/// <param name="time">Source of the timestamps.</param>
public sealed class FileLogProvider(FileLogWriter writer, TimeProvider time) : ILoggerProvider
{
    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName, writer, time);

    /// <inheritdoc />
    public void Dispose()
    {
    }

    private sealed class FileLogger(string category, FileLogWriter writer, TimeProvider time) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var stamp = time.GetUtcNow().UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
            writer.Write($"{stamp} {LogLine.Format(logLevel, category, eventId, formatter(state, exception), exception)}");
        }
    }
}
