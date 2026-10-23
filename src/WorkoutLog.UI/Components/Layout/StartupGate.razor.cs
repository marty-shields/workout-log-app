using Microsoft.Extensions.Logging;

namespace WorkoutLog.UI.Components.Layout;

/// <summary>Runs <see cref="Initialize"/> once, showing the Startup screen until it completes.</summary>
public partial class StartupGate
{
    private static partial class Log
    {
        [LoggerMessage(2001, LogLevel.Information, "Startup finished in {Milliseconds} ms")]
        public static partial void Started(ILogger logger, long milliseconds);

        [LoggerMessage(2002, LogLevel.Error, "Startup failed")]
        public static partial void Failed(ILogger logger, Exception exception);
    }
}
