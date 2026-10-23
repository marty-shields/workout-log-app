using Microsoft.Extensions.Logging;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Screens;

/// <summary>Logs a saved workout that can't be opened, and discarded edits with the workout id only. Deletes are logged by Core.</summary>
public partial class EditWorkoutPage
{
    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.EditWorkoutOpenFailed, LogLevel.Error, "Opening workout {WorkoutId} failed: exercise {ExerciseId} is unknown or the wrong type")]
        public static partial void OpenFailed(ILogger logger, int workoutId, int exerciseId);

        [LoggerMessage((int)UiEvent.EditsDiscarded, LogLevel.Information, "Edits to workout {WorkoutId} discarded")]
        public static partial void EditsDiscarded(ILogger logger, int workoutId);
    }
}
