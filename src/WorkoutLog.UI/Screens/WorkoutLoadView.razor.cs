using Microsoft.Extensions.Logging;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Screens;

/// <summary>Logs a failed focus on Try again; the screen stays as it is.</summary>
public partial class WorkoutLoadView
{
    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.WorkoutRetryFocusFailed, LogLevel.Warning, "Focusing Try again failed")]
        public static partial void RetryFocusFailed(ILogger logger, Exception exception);
    }
}
