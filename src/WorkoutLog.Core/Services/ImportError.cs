namespace WorkoutLog.Core.Services;

/// <summary>Why a file can't be imported.</summary>
public enum ImportError
{
    /// <summary>The file was read.</summary>
    None,

    /// <summary>Not JSON, or not a Workout Log export.</summary>
    NotAnExport,

    /// <summary>The file's format version is newer than the app's.</summary>
    NewerVersion,

    /// <summary>The file has no workouts.</summary>
    NoWorkouts,

    /// <summary>The file is over the size limit.</summary>
    TooLarge,

    /// <summary>Reading the file or this phone's workouts failed.</summary>
    Failed,
}
