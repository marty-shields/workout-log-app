namespace WorkoutLog.Core.Services;

/// <summary>A checked import file, before anything is written. The counts are 0 when <see cref="Error"/> isn't <see cref="ImportError.None"/>.</summary>
/// <param name="FileName">The file's name.</param>
/// <param name="Total">Workouts in the file.</param>
/// <param name="New">Workouts whose start no phone workout has.</param>
/// <param name="Already">Workouts already on the phone with the same contents.</param>
/// <param name="Different">Workouts on the phone with different contents.</param>
/// <param name="Skipped">Workouts that can't be imported.</param>
/// <param name="PhoneCount">Workouts on the phone.</param>
/// <param name="Error">Why the file can't be imported, or <see cref="ImportError.None"/>.</param>
public sealed record ImportReview(
    string FileName, int Total, int New, int Already, int Different, int Skipped, int PhoneCount, ImportError Error = ImportError.None)
{
    /// <summary>The checked workouts that <see cref="IImportService.ImportAsync"/> writes.</summary>
    internal ImportGroups Groups { get; init; } = ImportGroups.Empty;

    /// <summary>A file that can't be imported.</summary>
    public static ImportReview Rejected(string fileName, ImportError error) => new(fileName, 0, 0, 0, 0, 0, 0, error);
}
