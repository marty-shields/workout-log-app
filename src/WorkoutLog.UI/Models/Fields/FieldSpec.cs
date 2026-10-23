namespace WorkoutLog.UI.Models.Fields;

/// <summary>One field shown for a strength set or cardio entry.</summary>
/// <param name="Key">Key of the value in <see cref="Models.Workout.EntryState.Values"/>.</param>
/// <param name="Label">Field label, e.g. Reps or Assist.</param>
/// <param name="Kind">How the value is entered.</param>
/// <param name="Required">Whether the field must be filled to save.</param>
/// <param name="Unit">Unit shown after the label, e.g. kg.</param>
public sealed record FieldSpec(string Key, string Label, FieldKind Kind, bool Required = false, string? Unit = null)
{
    /// <summary>Label with the unit, e.g. Weight (kg).</summary>
    public string DisplayLabel => Unit is null ? Label : $"{Label} ({Unit})";
}
