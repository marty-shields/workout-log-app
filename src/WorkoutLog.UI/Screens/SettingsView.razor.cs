using Microsoft.Extensions.Logging;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Screens;

/// <summary>Logs a failed focus on Export or Import; the message stays.</summary>
public partial class SettingsView
{
    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.ExportFocusFailed, LogLevel.Warning, "Focusing Export failed")]
        public static partial void ExportFocusFailed(ILogger logger, Exception exception);

        [LoggerMessage((int)UiEvent.SettingsImportFocusFailed, LogLevel.Warning, "Focusing Import failed")]
        public static partial void ImportFocusFailed(ILogger logger, Exception exception);
    }
}
