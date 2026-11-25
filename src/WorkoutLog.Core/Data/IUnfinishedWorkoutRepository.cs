using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Data;

/// <summary>Stores the one unfinished workout. Each write is one transaction.</summary>
internal interface IUnfinishedWorkoutRepository
{
    /// <summary>Returns the unfinished workout, or null if there is none.</summary>
    Task<UnfinishedWorkout?> GetAsync();

    /// <summary>Replaces the unfinished workout. One with no exercises is deleted instead: it isn't worth recovering.</summary>
    Task ReplaceAsync(UnfinishedWorkout workout);

    /// <summary>Deletes the unfinished workout, if any.</summary>
    Task DeleteAsync();
}
