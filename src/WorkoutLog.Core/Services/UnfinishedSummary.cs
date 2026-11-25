using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Services;

/// <summary>What an unfinished workout holds, for the banner and the dialogs that offer to resume or discard it.</summary>
/// <param name="StartedAt">Start time, UTC.</param>
/// <param name="Exercises">Number of exercises.</param>
/// <param name="Sets">Number of sets, a cardio entry counting as one, including an open one still blank.</param>
public sealed record UnfinishedSummary(DateTimeOffset StartedAt, int Exercises, int Sets)
{
    /// <summary>The summary of <paramref name="workout"/>.</summary>
    public static UnfinishedSummary Of(UnfinishedWorkout workout)
    {
        ArgumentNullException.ThrowIfNull(workout);
        return new UnfinishedSummary(workout.StartedAt, workout.Exercises.Count, workout.Exercises.Sum(e => e.Entries.Count));
    }
}
