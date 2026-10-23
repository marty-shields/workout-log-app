using Microsoft.Extensions.Logging;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Components.Layout;

/// <summary>Shows the Error screen when its content throws an unhandled error, and logs the error.</summary>
public partial class AppErrorBoundary
{
    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.ScreenError, LogLevel.Error, "Unhandled error in a screen")]
        public static partial void Unhandled(ILogger logger, Exception exception);
    }
}
