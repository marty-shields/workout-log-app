using Microsoft.Extensions.Logging;

namespace WorkoutLog.UI.Components.Workout;

/// <summary>Logs a failed focus on a set that blocked Add set or opening another set; the workout stays open.</summary>
public partial class ExerciseCard
{
    private static partial class Log
    {
        [LoggerMessage(2401, LogLevel.Warning, "Focusing a blocked set failed")]
        public static partial void FocusFailed(ILogger logger, Exception exception);
    }
}
