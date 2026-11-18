using Microsoft.Extensions.Logging;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Screens;

/// <summary>Logs a load that failed, with no workout contents. Core logs which read failed and why.</summary>
public partial class ThisWeekPage
{
    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.ThisWeekLoadFailed, LogLevel.Warning, "Loading This week failed")]
        public static partial void LoadFailed(ILogger logger);

        [LoggerMessage((int)UiEvent.ThisWeekRecordsFailed, LogLevel.Warning, "Loading the records for This week failed; the card is left out")]
        public static partial void RecordsFailed(ILogger logger);
    }
}
