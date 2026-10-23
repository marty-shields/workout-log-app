using Microsoft.Extensions.Logging;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Screens;

/// <summary>Logs a failed focus on an exercise or on the first error; the workout stays open.</summary>
public partial class WorkoutView
{
    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.ExerciseFocusFailed, LogLevel.Warning, "Focusing an exercise failed")]
        public static partial void FocusFailed(ILogger logger, Exception exception);

        [LoggerMessage((int)UiEvent.ErrorFocusFailed, LogLevel.Warning, "Focusing the first error failed")]
        public static partial void ErrorFocusFailed(ILogger logger, Exception exception);
    }
}
