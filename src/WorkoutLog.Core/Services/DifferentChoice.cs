namespace WorkoutLog.Core.Services;

/// <summary>What <see cref="ImportMode.Add"/> does with a workout that is on the phone with different contents.</summary>
public enum DifferentChoice
{
    /// <summary>Keeps the phone's workout.</summary>
    KeepPhones,

    /// <summary>Replaces the phone's finish time, exercises, sets and entries with the file's.</summary>
    UseFiles,
}
