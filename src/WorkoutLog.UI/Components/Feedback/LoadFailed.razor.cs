using Microsoft.Extensions.Logging;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Components.Feedback;

/// <summary>Logs a failed focus on Try again; the error stays as it is.</summary>
public partial class LoadFailed
{
    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.LoadFailedFocusFailed, LogLevel.Warning, "Focusing Try again failed")]
        public static partial void RetryFocusFailed(ILogger logger, Exception exception);
    }
}
