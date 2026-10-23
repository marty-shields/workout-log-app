using Microsoft.Extensions.Logging;

namespace WorkoutLog.UI.Components.Layout;

/// <summary>Shows the Error screen when its content throws an unhandled error, and logs the error.</summary>
public partial class AppErrorBoundary
{
    private static partial class Log
    {
        [LoggerMessage(2101, LogLevel.Error, "Unhandled error in a screen")]
        public static partial void Unhandled(ILogger logger, Exception exception);
    }
}
