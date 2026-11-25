using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Services;

/// <summary>
/// Keeps the one new workout still open, as typed, so a crash or kill doesn't lose it, and reads it back to resume it or discard it.
/// A saved workout, the history, trends, records and the export never read it. Saving a new workout (<see cref="IWorkoutService.SaveNewAsync"/>)
/// deletes it in the same transaction.
/// </summary>
public interface IUnfinishedWorkoutService
{
    /// <summary>Returns the start and counts of the unfinished workout, for the banner and the dialogs that offer to resume or discard it.</summary>
    Task<UnfinishedSummaryResult> GetSummaryAsync();

    /// <summary>Returns the unfinished workout to resume. An exercise that is no longer in the exercise list is left out.</summary>
    Task<UnfinishedWorkoutResult> GetAsync();

    /// <summary>
    /// Replaces the unfinished workout with <paramref name="workout"/> in one transaction: after a failure the earlier one is still there.
    /// A workout with no exercises deletes it instead.
    /// </summary>
    Task<UnfinishedWrite> SaveAsync(UnfinishedWorkout workout);

    /// <summary>Deletes the unfinished workout. Done when there is none.</summary>
    Task<UnfinishedWrite> DiscardAsync();
}
