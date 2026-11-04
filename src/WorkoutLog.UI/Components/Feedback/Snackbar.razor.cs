using Microsoft.Extensions.Logging;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Components.Feedback;

/// <summary>Logs a failed focus on Undo, or a failed check for keyboard focus on it; the snackbar stays as it is.</summary>
public partial class Snackbar
{
    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.UndoFocusFailed, LogLevel.Warning, "Focusing Undo failed")]
        public static partial void UndoFocusFailed(ILogger logger, Exception exception);

        [LoggerMessage((int)UiEvent.UndoFocusCheckFailed, LogLevel.Warning, "Checking Undo for keyboard focus failed")]
        public static partial void FocusCheckFailed(ILogger logger, Exception exception);
    }
}
