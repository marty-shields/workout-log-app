using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
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
    public async Task<ReadResult<RecentRecords>> GetAsync()
    {
        var now = time.GetUtcNow();
        try
        {
            var workouts = await repository.GetAllAsync();
            var zone = time.LocalTimeZone;
            var from = LocalDate(now, zone).AddDays(1 - Days);
            bool InWindow(DateTimeOffset startedAt) => startedAt <= now && LocalDate(startedAt, zone) >= from;

            var definitions = catalog.All.ToDictionary(e => e.Id);
            var records = RecordSearch.Find(workouts, definitions, recordCheck, id => Log.UnknownExercise(logger, id), (id, exception) => Log.RecordCheckFailed(logger, id, exception))
                .Select(f => (Latest: f.Records.LastOrDefault(r => InWindow(r.Session.StartedAt)), f.Definition))
                .Where(f => f.Latest is not null)
                .Select(f => (Record: new ExerciseRecord(f.Definition.Id, f.Latest!.Session.StartedAt, f.Latest.Best), f.Definition.Name))
                .ToList();

            var recent = records
                .OrderByDescending(r => r.Record.StartedAt)
                .ThenBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
                .Take(MaxRecords)
                .Select(r => r.Record)
                .ToList();
            Log.Loaded(logger, recent.Count);
            return new RecentRecords(recent, now);
        }
        catch (Exception exception)
        {
            Log.LoadFailed(logger, exception);
            return new ReadFailed();
        }
    }

    private static DateOnly LocalDate(DateTimeOffset value, TimeZoneInfo zone) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(value, zone).DateTime);

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.RecentRecordsLoaded, LogLevel.Information, "Loaded {Count} recent records")]
        public static partial void Loaded(ILogger logger, int count);

        [LoggerMessage((int)CoreEvent.RecentRecordsLoadFailed, LogLevel.Warning, "Loading the recent records failed")]
        public static partial void LoadFailed(ILogger logger, Exception exception);

        [LoggerMessage((int)CoreEvent.RecentRecordsExerciseFailed, LogLevel.Warning, "Finding the records of exercise {ExerciseId} failed; left out")]
        public static partial void RecordCheckFailed(ILogger logger, int exerciseId, Exception exception);

        [LoggerMessage((int)CoreEvent.RecentRecordsUnknownExercise, LogLevel.Warning, "Exercise {ExerciseId} is not in the exercise list; left out")]
        public static partial void UnknownExercise(ILogger logger, int exerciseId);
    }
}
