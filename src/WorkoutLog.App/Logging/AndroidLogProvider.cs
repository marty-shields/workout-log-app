using Android.Util;
using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.App.Logging;

/// <summary>Writes log entries to logcat with the tag <see cref="Tag"/>. Any write error is ignored.</summary>
internal sealed class AndroidLogProvider : ILoggerProvider
{
    public const string Tag = "WorkoutLog";

    /// <summary>Longest text written in one logcat entry; longer text is split.</summary>
    private const int MaxEntryLength = 4000;

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName) => new AndroidLogger(categoryName);

    /// <inheritdoc />
    public void Dispose()
    {
    }

    private sealed class AndroidLogger(string category) : ILogger
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

            try
            {
                var text = LogLine.Format(logLevel, category, eventId, formatter(state, exception), exception);
                for (var start = 0; start < text.Length; start += MaxEntryLength)
                {
                    Android.Util.Log.WriteLine(Priority(logLevel), Tag, text.Substring(start, Math.Min(MaxEntryLength, text.Length - start)));
                }
            }
            catch (Exception)
            {
            }
        }

        private static LogPriority Priority(LogLevel level) => level switch
        {
            LogLevel.Trace => LogPriority.Verbose,
            LogLevel.Debug => LogPriority.Debug,
            LogLevel.Information => LogPriority.Info,
            LogLevel.Warning => LogPriority.Warn,
            LogLevel.Error => LogPriority.Error,
            _ => LogPriority.Assert,
        };
    }
}
