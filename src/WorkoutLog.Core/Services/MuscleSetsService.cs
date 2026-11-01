using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Consistency;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>
/// Reads the last 7 days' set counts through <see cref="IWorkoutRepository"/>, adds them up per group with <see cref="IMuscleGroupMap"/>
/// and logs the outcome.
/// </summary>
internal sealed partial class MuscleSetsService(
    IWorkoutRepository repository,
    IExerciseCatalog catalog,
    IMuscleGroupMap groups,
    TimeProvider time,
    ILogger<MuscleSetsService> logger) : IMuscleSetsService
{
    private readonly Dictionary<int, ExerciseDefinition> definitions = catalog.All.ToDictionary(e => e.Id);

    /// <inheritdoc />
    public async Task<MuscleSetsResult> GetAsync()
    {
        IReadOnlyList<ExerciseSetCount> counts;
        try
        {
            var now = time.GetUtcNow();
            var zone = time.LocalTimeZone;
            var from = LocalWeek.First(TimeZoneInfo.ConvertTime(now, zone).Date.AddDays(-6), zone);
            counts = await repository.GetSetCountsAsync(from, now);
        }
        catch (Exception exception)
        {
            Log.LoadFailed(logger, exception);
            return new MuscleSetsResult([], Failed: true);
        }

        var sets = Enum.GetValues<MuscleGroup>().ToDictionary(g => g, _ => 0);
        var unknown = new HashSet<int>();
        var workouts = new HashSet<int>();
        foreach (var count in counts)
        {
            if (!definitions.TryGetValue(count.ExerciseId, out var definition))
            {
                if (unknown.Add(count.ExerciseId))
                {
                    Log.UnknownExercise(logger, count.ExerciseId);
                }

                continue;
            }

            if (definition is { Type: ExerciseType.Strength, PrimaryMuscle: { } muscle })
            {
                sets[groups.GroupOf(muscle)] += count.Sets;
                workouts.Add(count.WorkoutId);
            }
        }

        Log.Loaded(logger, workouts.Count);
        return new MuscleSetsResult(Enum.GetValues<MuscleGroup>().Select(g => new MuscleGroupCount(g, sets[g])).ToList(), Failed: false);
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.MuscleSetsLoaded, LogLevel.Information, "Loaded the sets per muscle group from {Workouts} workouts")]
        public static partial void Loaded(ILogger logger, int workouts);

        [LoggerMessage((int)CoreEvent.MuscleSetsLoadFailed, LogLevel.Warning, "Loading the sets per muscle group failed")]
        public static partial void LoadFailed(ILogger logger, Exception exception);

        [LoggerMessage((int)CoreEvent.MuscleSetsUnknownExercise, LogLevel.Warning, "Exercise {ExerciseId} is not in the exercise list; left out")]
        public static partial void UnknownExercise(ILogger logger, int exerciseId);
    }
}
