using Microsoft.Extensions.Logging;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Screens;

/// <summary>Logs a failed focus on Export; the message stays.</summary>
public partial class SettingsView
{
    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.ExportFocusFailed, LogLevel.Warning, "Focusing Export failed")]
        public static partial void ExportFocusFailed(ILogger logger, Exception exception);
    }
}
