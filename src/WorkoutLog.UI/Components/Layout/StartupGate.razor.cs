using Microsoft.Extensions.Logging;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Components.Layout;

/// <summary>Runs <see cref="Initialize"/> once, showing the Startup screen until it completes.</summary>
public partial class StartupGate
{
    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.StartupFinished, LogLevel.Information, "Startup finished in {Milliseconds} ms")]
        public static partial void Started(ILogger logger, long milliseconds);

        [LoggerMessage((int)UiEvent.StartupFailed, LogLevel.Error, "Startup failed")]
        public static partial void Failed(ILogger logger, Exception exception);
    }
}
