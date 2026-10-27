using WorkoutLog.Core.Services;

namespace WorkoutLog.App.Export;

/// <summary>The app's display version and cache folder, from MAUI.</summary>
internal sealed class MauiExportEnvironment : IExportEnvironment
{
    /// <inheritdoc />
    public string AppVersion => AppInfo.Current.VersionString;

    /// <inheritdoc />
    public string CacheFolder => FileSystem.CacheDirectory;
}
