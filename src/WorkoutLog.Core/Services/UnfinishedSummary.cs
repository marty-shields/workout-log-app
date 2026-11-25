using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Services;

/// <summary>What an unfinished workout holds, for the banner and the dialogs that offer to resume or discard it.</summary>
/// <param name="StartedAt">Start time, UTC.</param>
/// <param name="Exercises">Number of exercises.</param>
/// <param name="Sets">Number of strength sets, including an open one still blank. A cardio entry is not a set.</param>
public sealed record UnfinishedSummary(DateTimeOffset StartedAt, int Exercises, int Sets)
{
    /// <summary>The summary of <paramref name="workout"/>. An exercise that isn't in the list adds no sets.</summary>
    public static UnfinishedSummary Of(UnfinishedWorkout workout, IExerciseCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(workout);
        ArgumentNullException.ThrowIfNull(catalog);
        var strength = catalog.All.Where(e => e.Type == ExerciseType.Strength).Select(e => e.Id).ToHashSet();
        return new UnfinishedSummary(workout.StartedAt, workout.Exercises.Count, workout.Exercises.Where(e => strength.Contains(e.ExerciseId)).Sum(e => e.Entries.Count));
    }
}
