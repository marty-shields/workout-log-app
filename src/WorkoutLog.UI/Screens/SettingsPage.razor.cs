using Microsoft.Extensions.Logging;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Screens;

/// <summary>Logs a failed share or file pick; Settings shows the failed message.</summary>
public partial class SettingsPage
{
    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.ShareFailed, LogLevel.Error, "Opening the share sheet failed")]
        public static partial void ShareFailed(ILogger logger, Exception exception);

        [LoggerMessage((int)UiEvent.SettingsPickFailed, LogLevel.Error, "Picking or opening the import file failed")]
        public static partial void PickFailed(ILogger logger, Exception exception);
    }
}
