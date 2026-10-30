using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>
/// Reads every saved workout through <see cref="IWorkoutRepository"/>, finds each exercise's records with <see cref="IRecordCheck"/>,
/// keeps the latest in the window and logs the outcome.
/// </summary>
internal sealed partial class RecentRecordsService(
    IWorkoutRepository repository,
    IExerciseCatalog catalog,
    IRecordCheck recordCheck,
    TimeProvider time,
    ILogger<RecentRecordsService> logger) : IRecentRecordsService
{
    private const int Days = 30;
    private const int MaxRecords = 3;

    /// <inheritdoc />
    public async Task<RecentRecordsResult> GetAsync()
    {
        var now = time.GetUtcNow();
        try
        {
            var workouts = await repository.GetAllAsync();
            var zone = time.LocalTimeZone;
            var from = LocalDate(now, zone).AddDays(1 - Days);
            bool InWindow(DateTimeOffset startedAt) => startedAt <= now && LocalDate(startedAt, zone) >= from;

            var definitions = catalog.All.ToDictionary(e => e.Id);
            var records = new List<(ExerciseRecord Record, string Name)>();
            var sessions = workouts.SelectMany(w => w.Exercises.Select(e => new WorkoutSession(w.Id, w.StartedAt, e)));
            foreach (var exercise in sessions.GroupBy(s => s.Exercise.ExerciseId))
            {
                if (!definitions.TryGetValue(exercise.Key, out var definition))
                {
                    Log.UnknownExercise(logger, exercise.Key);
                    continue;
                }

                if (recordCheck.Records(definition, exercise).LastOrDefault(r => InWindow(r.Session.StartedAt)) is { } latest)
                {
                    records.Add((new ExerciseRecord(exercise.Key, latest.Session.StartedAt, latest.Best), definition.Name));
                }
            }

            var recent = records
                .OrderByDescending(r => r.Record.StartedAt)
                .ThenBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
                .Take(MaxRecords)
                .Select(r => r.Record)
                .ToList();
            Log.Loaded(logger, recent.Count);
            return new RecentRecordsResult(recent, now, Failed: false);
        }
        catch (Exception exception)
        {
            Log.LoadFailed(logger, exception);
            return new RecentRecordsResult([], now, Failed: true);
        }
    }

    private static DateOnly LocalDate(DateTimeOffset value, TimeZoneInfo zone) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(value, zone).DateTime);

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.RecentRecordsLoaded, LogLevel.Information, "Loaded {Count} recent records")]
        public static partial void Loaded(ILogger logger, int count);

        [LoggerMessage((int)CoreEvent.RecentRecordsLoadFailed, LogLevel.Warning, "Loading the recent records failed")]
        public static partial void LoadFailed(ILogger logger, Exception exception);

        [LoggerMessage((int)CoreEvent.RecentRecordsUnknownExercise, LogLevel.Warning, "Exercise {ExerciseId} is not in the exercise list; left out")]
        public static partial void UnknownExercise(ILogger logger, int exerciseId);
    }
}
