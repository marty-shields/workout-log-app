namespace WorkoutLog.UI.Models.Workout;

/// <summary>Values of one strength set or cardio entry while it is edited.</summary>
public sealed class EntryState
{
    /// <summary>Entered text per <see cref="Fields.FieldSpec.Key"/>. Duration is stored as h:mm:ss, pace as m:ss.</summary>
    public Dictionary<string, string?> Values { get; } = new();

    /// <summary>Validation message per <see cref="Fields.FieldSpec.Key"/>, for fields at fault.</summary>
    public Dictionary<string, string> Errors { get; } = new();

    /// <summary>Re-check on each change (D174). Set when a check fails; cleared when the set passes Add set or opening another set, or the workout passes Save.</summary>
    public bool CheckOnChange { get; set; }
}
