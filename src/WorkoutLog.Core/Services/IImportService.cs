namespace WorkoutLog.Core.Services;

/// <summary>Imports workouts from a file made by Export (FR-52).</summary>
public interface IImportService
{
    /// <summary>
    /// Reads and checks the whole file, then groups each workout against the phone's. Writes nothing.
    /// A file that can't be imported, or a failed read of the file or the phone's workouts, returns the review with its <see cref="ImportReview.Error"/>.
    /// The caller owns <paramref name="stream"/>.
    /// </summary>
    Task<ImportReview> ReadAsync(Stream stream, string fileName);

    /// <summary>
    /// Writes <paramref name="review"/>'s workouts in one transaction: all or nothing.
    /// <see cref="ImportMode.Add"/> adds the new ones and, with <see cref="DifferentChoice.UseFiles"/>, replaces the different ones;
    /// <see cref="ImportMode.ReplaceAll"/> deletes every phone workout and adds every one that can be imported.
    /// Throws <see cref="ArgumentException"/> for a review with an error, or for <see cref="ImportMode.ReplaceAll"/> with no workout that can be imported.
    /// </summary>
    Task<ImportResult> ImportAsync(ImportReview review, ImportMode mode, DifferentChoice different);
}
