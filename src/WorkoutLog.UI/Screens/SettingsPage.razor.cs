using Microsoft.Extensions.Logging;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Screens;

/// <summary>Logs a failed share; Settings shows the failed message.</summary>
public partial class SettingsPage
{
    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.ShareFailed, LogLevel.Error, "Opening the share sheet failed")]
        public static partial void ShareFailed(ILogger logger, Exception exception);
    }
}
