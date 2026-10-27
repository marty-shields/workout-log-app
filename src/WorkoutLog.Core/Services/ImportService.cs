using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Import;
using WorkoutLog.Core.Import.Checks;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>
/// Reads the file with <see cref="IImportFileReader"/>, checks each workout with <see cref="IImportChecker"/>, groups it against
/// <see cref="IWorkoutRepository.GetAllAsync"/> with <see cref="IWorkoutContentComparer"/>, and writes through <see cref="IUnitOfWork"/>.
/// </summary>
internal sealed partial class ImportService(
    IImportFileReader reader,
    IImportChecker checker,
    IWorkoutRepository repository,
    IWorkoutContentComparer comparer,
    IUnitOfWork unitOfWork,
    ILogger<ImportService> logger) : IImportService
{
    /// <inheritdoc />
    public async Task<ImportReview> ReadAsync(Stream stream, string fileName)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ImportFileRead file;
        try
        {
            file = await reader.ReadAsync(stream);
        }
        catch (Exception exception)
        {
            Log.FileUnreadable(logger, ImportError.Failed, exception);
            return ImportReview.Rejected(fileName, ImportError.Failed);
        }

        if (file.Error != ImportError.None)
        {
            if (file.Exception is { } exception)
            {
                Log.FileUnreadable(logger, file.Error, exception);
            }
            else
            {
                Log.FileRejected(logger, file.Error);
            }

            return ImportReview.Rejected(fileName, file.Error);
        }

        IReadOnlyList<Workout> saved;
        try
        {
            saved = await repository.GetAllAsync();
        }
        catch (Exception exception)
        {
            Log.PhoneReadFailed(logger, exception);
            return ImportReview.Rejected(fileName, ImportError.Failed);
        }

        return Group(fileName, file.Workouts, saved);
    }

    /// <inheritdoc />
    public async Task<ImportResult> ImportAsync(ImportReview review, ImportMode mode, DifferentChoice different)
    {
        ArgumentNullException.ThrowIfNull(review);
        if (review.Error != ImportError.None)
        {
            throw new ArgumentException($"The file can't be imported: {review.Error}", nameof(review));
        }

        var groups = review.Groups;
        if (mode == ImportMode.ReplaceAll && groups.New.Count + groups.Already.Count + groups.Different.Count == 0)
        {
            throw new ArgumentException("Replace all needs a workout that can be imported", nameof(review));
        }

        IReadOnlyList<NewWorkout> added = mode == ImportMode.ReplaceAll ? [.. groups.New, .. groups.Already, .. groups.Different] : groups.New;
        IReadOnlyList<NewWorkout> replaced = mode == ImportMode.Add && different == DifferentChoice.UseFiles ? groups.Different : [];
        try
        {
            await unitOfWork.RunAsync(writer =>
            {
                if (mode == ImportMode.ReplaceAll)
                {
                    writer.DeleteAll();
                }

                foreach (var workout in added)
                {
                    writer.Add(workout);
                }

                foreach (var workout in replaced)
                {
                    writer.Replace(workout);
                }
            });
        }
        catch (Exception exception)
        {
            Log.WriteFailed(logger, exception);
            return new ImportResult(ImportStatus.Failed, 0, 0, 0);
        }

        var how = mode == ImportMode.ReplaceAll ? $"{mode}" : $"{mode} and {different}";
        Log.Imported(logger, how, review.New, review.Already, review.Different, review.Skipped);
        return new ImportResult(ImportStatus.Imported, added.Count, replaced.Count, review.Skipped);
    }

    private ImportReview Group(string fileName, IReadOnlyList<ImportWorkout> workouts, IReadOnlyList<Workout> saved)
    {
        var savedByStart = saved.ToDictionary(w => w.StartedAt);
        var context = ImportContext.For(workouts);
        List<NewWorkout> added = [], already = [], different = [];
        foreach (var workout in workouts)
        {
            var reasons = checker.Check(workout, context);
            if (reasons.Count > 0)
            {
                Log.WorkoutSkipped(logger, workout.Position, string.Join(", ", reasons));
                continue;
            }

            var imported = workout.ToNewWorkout();
            var group = savedByStart.TryGetValue(imported.StartedAt, out var phone)
                ? comparer.AreSame(phone, imported) ? already : different
                : added;
            group.Add(imported);
        }

        return new ImportReview(
            fileName, workouts.Count, added.Count, already.Count, different.Count,
            workouts.Count - added.Count - already.Count - different.Count, saved.Count)
        {
            Groups = new ImportGroups(added, already, different),
        };
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.ImportDone, LogLevel.Information,
            "Imported with {How}: {NewCount} new, {AlreadyCount} already on the phone, {DifferentCount} different, {SkippedCount} can't be imported")]
        public static partial void Imported(ILogger logger, string how, int newCount, int alreadyCount, int differentCount, int skippedCount);

        [LoggerMessage((int)CoreEvent.ImportWorkoutSkipped, LogLevel.Warning, "Workout {Position} in the file can't be imported: {Reasons}")]
        public static partial void WorkoutSkipped(ILogger logger, int position, string reasons);

        [LoggerMessage((int)CoreEvent.ImportFileUnreadable, LogLevel.Error, "The import file can't be imported: {Error}")]
        public static partial void FileUnreadable(ILogger logger, ImportError error, Exception exception);

        [LoggerMessage((int)CoreEvent.ImportFileRejected, LogLevel.Warning, "The import file can't be imported: {Error}")]
        public static partial void FileRejected(ILogger logger, ImportError error);

        [LoggerMessage((int)CoreEvent.ImportPhoneReadFailed, LogLevel.Error, "Reading the phone's workouts for an import failed")]
        public static partial void PhoneReadFailed(ILogger logger, Exception exception);

        [LoggerMessage((int)CoreEvent.ImportWriteFailed, LogLevel.Error, "Writing the import failed; nothing was written")]
        public static partial void WriteFailed(ILogger logger, Exception exception);
    }
}
