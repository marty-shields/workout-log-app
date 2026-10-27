using Microsoft.Extensions.Logging;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Screens;

/// <summary>Logs a failed focus on Import; the message stays.</summary>
public partial class ImportReviewView
{
    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.ReviewImportFocusFailed, LogLevel.Warning, "Focusing Import on the review failed")]
        public static partial void ImportFocusFailed(ILogger logger, Exception exception);
    }
}
