using WorkoutLog.UI.Versioning;

namespace WorkoutLog.App.Versioning;

/// <summary>Reads the version from MAUI <see cref="AppInfo"/>.</summary>
internal sealed class MauiAppVersion : IAppVersion
{
    /// <inheritdoc />
    public string Current => AppInfo.Current.VersionString;
}
