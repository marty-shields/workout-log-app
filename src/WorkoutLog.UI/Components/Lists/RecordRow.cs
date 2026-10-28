namespace WorkoutLog.UI.Components.Lists;

/// <summary>One record in a <see cref="RecordsCard"/>.</summary>
/// <param name="Label">What the record is, e.g. Heaviest weight.</param>
/// <param name="Value">The record, e.g. 70kg × 3; none shows —.</param>
/// <param name="Date">When it was set, e.g. 20 Oct; not shown when there is no value.</param>
public sealed record RecordRow(string Label, string? Value, string? Date);
