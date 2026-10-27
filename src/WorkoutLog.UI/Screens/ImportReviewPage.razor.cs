using Microsoft.Extensions.Logging;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Screens;

/// <summary>Logs a failed file pick; the review shows the failed message.</summary>
public partial class ImportReviewPage
{
    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.ReviewPickFailed, LogLevel.Error, "Picking or opening another import file failed")]
        public static partial void PickFailed(ILogger logger, Exception exception);
    }
}
