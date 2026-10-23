using Microsoft.Extensions.Logging;

namespace WorkoutLog.UI.Screens;

/// <summary>Logs a discarded new workout, with its exercise count only.</summary>
public partial class NewWorkoutPage
{
    private static partial class Log
    {
        [LoggerMessage(2701, LogLevel.Information, "New workout discarded with {ExerciseCount} exercises")]
        public static partial void Discarded(ILogger logger, int exerciseCount);
    }
}
