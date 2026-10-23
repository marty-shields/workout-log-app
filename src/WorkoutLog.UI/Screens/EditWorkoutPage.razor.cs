using Microsoft.Extensions.Logging;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Screens;

/// <summary>Logs a saved workout that can't be opened; the load failure shows instead.</summary>
public partial class EditWorkoutPage
{
    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.EditWorkoutOpenFailed, LogLevel.Error, "Opening workout {WorkoutId} failed: exercise {ExerciseId} is unknown or the wrong type")]
        public static partial void OpenFailed(ILogger logger, int workoutId, int exerciseId);
    }
}
