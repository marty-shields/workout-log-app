using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary>An import service for the stories app, which shows views only: every call throws.</summary>
public sealed class SampleImportService : IImportService
{
    /// <inheritdoc />
    public Task<ImportReview> ReadAsync(Stream stream, string fileName) => throw new NotSupportedException("The stories don't import.");

    /// <inheritdoc />
    public Task<ImportResult> ImportAsync(ImportReview review, ImportMode mode, DifferentChoice different) =>
        throw new NotSupportedException("The stories don't import.");
}
