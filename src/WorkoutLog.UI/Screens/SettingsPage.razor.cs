using Microsoft.Extensions.Logging;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Screens;

/// <summary>Logs a failed share, file pick or theme read or save. A share or pick failure shows the failed message; a theme failure keeps the previous choice.</summary>
public partial class SettingsPage
{
    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.ShareFailed, LogLevel.Error, "Opening the share sheet failed")]
        public static partial void ShareFailed(ILogger logger, Exception exception);

        [LoggerMessage((int)UiEvent.SettingsPickFailed, LogLevel.Error, "Picking or opening the import file failed")]
        public static partial void PickFailed(ILogger logger, Exception exception);

        [LoggerMessage((int)UiEvent.ThemeLoadFailed, LogLevel.Warning, "Reading the saved theme failed")]
        public static partial void ThemeLoadFailed(ILogger logger, Exception exception);

        [LoggerMessage((int)UiEvent.ThemeNotSaved, LogLevel.Warning, "Storage refused the saved theme; it applies until the app closes")]
        public static partial void ThemeNotSaved(ILogger logger);

        [LoggerMessage((int)UiEvent.ThemeSaveFailed, LogLevel.Warning, "Saving the theme failed")]
        public static partial void ThemeSaveFailed(ILogger logger, Exception exception);
    }
}
