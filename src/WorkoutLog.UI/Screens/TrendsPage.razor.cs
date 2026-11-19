using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.UI.Logging;
using WorkoutLog.UI.Models.Workouts;

namespace WorkoutLog.UI.Screens;

/// <summary>A record tapped on Trends.</summary>
/// <param name="ExerciseId">The exercise the record belongs to.</param>
/// <param name="Period">The week, month or year shown, which back returns to.</param>
public readonly record struct RecordOpened(int ExerciseId, Period Period);

/// <summary>A day, week row or bar tapped on Trends.</summary>
/// <param name="Dates">The days to show in Workouts.</param>
/// <param name="Period">The week, month or year shown, which back returns to.</param>
public readonly record struct WorkoutsOpened(DateSpan Dates, Period Period);

/// <summary>Logs a load that failed, with no workout contents. Core logs which read failed and why.</summary>
public partial class TrendsPage
{
    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.TrendsLoadFailed, LogLevel.Warning, "Loading Trends failed")]
        public static partial void LoadFailed(ILogger logger);

        [LoggerMessage((int)UiEvent.TrendsRecordsFailed, LogLevel.Warning, "Loading the records for Trends failed; the card is empty")]
        public static partial void RecordsFailed(ILogger logger);
    }
}
