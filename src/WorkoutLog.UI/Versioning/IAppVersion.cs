namespace WorkoutLog.UI.Versioning;

/// <summary>The installed app's version. Implemented by the host.</summary>
public interface IAppVersion
{
    /// <summary>The display version, such as "1.4.2". Dev builds show MAUI's default, "1.0".</summary>
    string Current { get; }
}
