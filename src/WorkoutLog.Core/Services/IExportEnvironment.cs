namespace WorkoutLog.Core.Services;

/// <summary>Values from the host that the export needs. Implemented by the host.</summary>
public interface IExportEnvironment
{
    /// <summary>The app's display version, written to the file.</summary>
    string AppVersion { get; }

    /// <summary>Full path of the folder the file is written to.</summary>
    string CacheFolder { get; }
}
