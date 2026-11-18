using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Consistency;
using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>Reads workouts through <see cref="IWorkoutRepository"/>, picks out a period's records, sets and exercises and logs the outcome.</summary>
internal sealed partial class PeriodTrainingService(
    IWorkoutRepository repository,
    IExerciseCatalog catalog,
    IRecordCheck recordCheck,
    MuscleGroupMap groups,
    TimeProvider time,
    ILogger<PeriodTrainingService> logger) : IPeriodTrainingService
{
    private const int MostTrainedCount = 5;
    private const int MonthsInYear = 12;

    private readonly Dictionary<int, ExerciseDefinition> definitions = catalog.All.ToDictionary(e => e.Id);

    /// <inheritdoc />
    public async Task<ReadResult<PeriodRecords>> GetRecordsAsync(Period period)
    {
        PeriodRecords result;
        try
        {
            var now = time.GetUtcNow();
            var zone = time.LocalTimeZone;
            var workouts = await repository.GetAllAsync();
            var records = RecordSearch.Find(workouts, definitions, recordCheck, id => Log.RecordsUnknownExercise(logger, id), (id, exception) => Log.RecordsExerciseFailed(logger, id, exception))
                .SelectMany(f => f.Records
                    .Where(r => IsIn(period, r.Session.StartedAt, now, zone))
                    .Select(r => (Record: new ExerciseRecord(f.Definition.Id, r.Session.StartedAt, r.Best), f.Definition.Name)))
                .OrderByDescending(r => r.Record.StartedAt)
                .ThenBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
                .Select(r => r.Record)
                .ToList();
            var perMonth = period.Kind == PeriodKind.Year
                ? Enumerable.Range(1, MonthsInYear).Select(month => records.Count(r => LocalDate(r.StartedAt, zone).Month == month)).ToList()
                : [];
            result = new PeriodRecords(records, perMonth);
        }
        catch (Exception exception)
        {
            Log.RecordsLoadFailed(logger, exception, period.Kind, period.Start);
            return new ReadFailed();
        }

        Log.RecordsLoaded(logger, result.Records.Count, period.Kind, period.Start);
        return result;
    }

    /// <inheritdoc />
    public async Task<ReadResult<IReadOnlyList<MuscleGroupCount>>> GetMuscleSetsAsync(Period period)
    {
        IReadOnlyList<ExerciseSetCount> counts;
        try
        {
            var now = time.GetUtcNow();
            var zone = time.LocalTimeZone;
            var to = LocalWeek.Start(period.End.AddDays(1), zone).AddTicks(-1);
            counts = await repository.GetSetCountsAsync(LocalWeek.Start(period.Start, zone), to < now ? to : now);
        }
        catch (Exception exception)
        {
            Log.MuscleSetsLoadFailed(logger, exception, period.Kind, period.Start);
            return new ReadFailed();
        }

        var (result, workouts) = MuscleSetTally.Count(counts, definitions, groups, id => Log.MuscleSetsUnknownExercise(logger, id));
        Log.MuscleSetsLoaded(logger, workouts, period.Kind, period.Start);
        return new ReadResult<IReadOnlyList<MuscleGroupCount>>(result);
    }

    /// <inheritdoc />
    public async Task<ReadResult<IReadOnlyList<TrainedExercise>>> GetMostTrainedAsync(Period period)
    {
        List<TrainedExercise> result;
        try
        {
            var now = time.GetUtcNow();
            var zone = time.LocalTimeZone;
            var workouts = (await repository.GetAllAsync()).Where(w => IsIn(period, w.StartedAt, now, zone)).ToList();
            var counts = workouts
                .SelectMany(w => w.Exercises.Select(e => e.ExerciseId))
                .GroupBy(id => id)
                .ToList();
            foreach (var unknown in counts.Where(g => !definitions.ContainsKey(g.Key)))
            {
                Log.MostTrainedUnknownExercise(logger, unknown.Key);
            }

            result = counts
                .Where(g => definitions.ContainsKey(g.Key))
                .Select(g => (Definition: definitions[g.Key], Sessions: g.Count()))
                .OrderByDescending(e => e.Sessions)
                .ThenBy(e => e.Definition.Name, StringComparer.OrdinalIgnoreCase)
                .Take(MostTrainedCount)
                .Select(e => new TrainedExercise(e.Definition.Id, e.Sessions))
                .ToList();
        }
        catch (Exception exception)
        {
            Log.MostTrainedLoadFailed(logger, exception, period.Kind, period.Start);
            return new ReadFailed();
        }

        Log.MostTrainedLoaded(logger, result.Count, period.Kind, period.Start);
        return new ReadResult<IReadOnlyList<TrainedExercise>>(result);
    }

    private static bool IsIn(Period period, DateTimeOffset startedAt, DateTimeOffset now, TimeZoneInfo zone) =>
        startedAt <= now && period.Contains(LocalDate(startedAt, zone));

    private static DateOnly LocalDate(DateTimeOffset value, TimeZoneInfo zone) => DateOnly.FromDateTime(LocalWeek.Local(value, zone));

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.PeriodRecordsLoaded, LogLevel.Information, "Loaded {Count} records for {Kind} from {Start:O}")]
        public static partial void RecordsLoaded(ILogger logger, int count, PeriodKind kind, DateOnly start);

        [LoggerMessage((int)CoreEvent.PeriodRecordsLoadFailed, LogLevel.Warning, "Loading the records for {Kind} from {Start:O} failed")]
        public static partial void RecordsLoadFailed(ILogger logger, Exception exception, PeriodKind kind, DateOnly start);

        [LoggerMessage((int)CoreEvent.PeriodRecordsUnknownExercise, LogLevel.Warning, "Exercise {ExerciseId} is not in the exercise list; left out")]
        public static partial void RecordsUnknownExercise(ILogger logger, int exerciseId);

        [LoggerMessage((int)CoreEvent.PeriodRecordsExerciseFailed, LogLevel.Warning, "Finding the records of exercise {ExerciseId} failed; left out")]
        public static partial void RecordsExerciseFailed(ILogger logger, int exerciseId, Exception exception);

        [LoggerMessage((int)CoreEvent.PeriodMuscleSetsLoaded, LogLevel.Information, "Loaded the sets per muscle group from {Workouts} workouts for {Kind} from {Start:O}")]
        public static partial void MuscleSetsLoaded(ILogger logger, int workouts, PeriodKind kind, DateOnly start);

        [LoggerMessage((int)CoreEvent.PeriodMuscleSetsLoadFailed, LogLevel.Warning, "Loading the sets per muscle group for {Kind} from {Start:O} failed")]
        public static partial void MuscleSetsLoadFailed(ILogger logger, Exception exception, PeriodKind kind, DateOnly start);

        [LoggerMessage((int)CoreEvent.PeriodMuscleSetsUnknownExercise, LogLevel.Warning, "Exercise {ExerciseId} is not in the exercise list; left out")]
        public static partial void MuscleSetsUnknownExercise(ILogger logger, int exerciseId);

        [LoggerMessage((int)CoreEvent.PeriodMostTrainedLoaded, LogLevel.Information, "Loaded {Count} most trained exercises for {Kind} from {Start:O}")]
        public static partial void MostTrainedLoaded(ILogger logger, int count, PeriodKind kind, DateOnly start);

        [LoggerMessage((int)CoreEvent.PeriodMostTrainedLoadFailed, LogLevel.Warning, "Loading the most trained exercises for {Kind} from {Start:O} failed")]
        public static partial void MostTrainedLoadFailed(ILogger logger, Exception exception, PeriodKind kind, DateOnly start);

        [LoggerMessage((int)CoreEvent.PeriodMostTrainedUnknownExercise, LogLevel.Warning, "Exercise {ExerciseId} is not in the exercise list; left out")]
        public static partial void MostTrainedUnknownExercise(ILogger logger, int exerciseId);
    }
}
