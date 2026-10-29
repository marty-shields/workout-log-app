using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Export;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>Reads every workout through <see cref="IWorkoutRepository"/>, writes the export file and logs the outcome.</summary>
internal sealed partial class ExportService(
    IWorkoutRepository repository,
    IExerciseCatalog catalog,
    IExportEnvironment environment,
    TimeProvider time,
    ILogger<ExportService> logger) : IExportService
{
    /// <inheritdoc />
    public async Task<ExportCount> CountAsync()
    {
        try
        {
            return new ExportCount((await repository.ListAsync()).Count, Failed: false);
        }
        catch (Exception exception)
        {
            Log.CountFailed(logger, exception);
            return new ExportCount(0, Failed: true);
        }
    }

    /// <inheritdoc />
    public async Task<ExportResult> ExportAsync()
    {
        try
        {
            var workouts = await repository.GetAllAsync();
            var now = time.GetUtcNow();
            var bytes = JsonSerializer.SerializeToUtf8Bytes(ToFile(workouts, now), ExportFormat.Options);
            if (bytes.Length > ExportFormat.MaxBytes)
            {
                Log.TooLarge(logger, workouts.Count, bytes.Length);
                return new ExportResult(ExportStatus.TooLarge, null);
            }

            var path = Path.Combine(environment.CacheFolder, FileName(now));
            await File.WriteAllBytesAsync(path, bytes);
            Log.Written(logger, workouts.Count);
            return new ExportResult(ExportStatus.Exported, path);
        }
        catch (Exception exception)
        {
            Log.Failed(logger, exception);
            return new ExportResult(ExportStatus.Failed, null);
        }
    }

    /// <summary><c>workout-log-YYYY-MM-DD-HHmm.json</c> in the phone's time zone.</summary>
    private string FileName(DateTimeOffset now) =>
        $"workout-log-{TimeZoneInfo.ConvertTime(now, time.LocalTimeZone).ToString("yyyy-MM-dd-HHmm", CultureInfo.InvariantCulture)}.json";

    private ExportFile ToFile(IReadOnlyList<Workout> workouts, DateTimeOffset now)
    {
        var exercises = catalog.All.ToDictionary(e => e.Id);
        foreach (var id in workouts.SelectMany(w => w.Exercises).Select(e => e.ExerciseId).Distinct().Where(id => !exercises.ContainsKey(id)))
        {
            Log.UnknownExercise(logger, id);
        }

        return new ExportFile(
            ExportFormat.Version,
            now,
            environment.AppVersion,
            workouts.Select(w => new ExportWorkout(w.StartedAt, w.FinishedAt, w.Exercises.Select(e => ToExercise(e, exercises)).ToList())).ToList());
    }

    private static ExportExercise ToExercise(WorkoutExercise exercise, Dictionary<int, ExerciseDefinition> exercises)
    {
        var info = exercises.TryGetValue(exercise.ExerciseId, out var definition)
            ? new ExportExerciseInfo(definition.Id, definition.Name, definition.Type, definition.DistanceUnit, definition.RateUnit, definition.PaceUnit)
            : new ExportExerciseInfo(exercise.ExerciseId);
        return exercise switch
        {
            StrengthExercise strength => new ExportExercise(info, strength.Sets.Select(s => new ExportSet(s.Reps, s.WeightKg, s.Duration, s.Distance)).ToList(), null),
            CardioExercise { Entry: var e } => new ExportExercise(info, null, new ExportEntry(e.Duration, e.Distance, e.Floors, e.Level, e.Incline, e.Calories, e.Rate)),
            _ => throw new ArgumentOutOfRangeException(nameof(exercise), $"Unknown exercise type {exercise.GetType().Name}"),
        };
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.ExportWritten, LogLevel.Information, "Export written with {Count} workouts")]
        public static partial void Written(ILogger logger, int count);

        [LoggerMessage((int)CoreEvent.ExportTooLarge, LogLevel.Warning, "Export of {Count} workouts is {Bytes} bytes, over the limit; not written")]
        public static partial void TooLarge(ILogger logger, int count, int bytes);

        [LoggerMessage((int)CoreEvent.ExportFailed, LogLevel.Error, "Exporting the workouts failed")]
        public static partial void Failed(ILogger logger, Exception exception);

        [LoggerMessage((int)CoreEvent.ExportUnknownExercise, LogLevel.Warning, "Exercise {ExerciseId} is not in the exercise list; exported with its id only")]
        public static partial void UnknownExercise(ILogger logger, int exerciseId);

        [LoggerMessage((int)CoreEvent.ExportCountFailed, LogLevel.Error, "Counting the workouts to export failed")]
        public static partial void CountFailed(ILogger logger, Exception exception);
    }
}
