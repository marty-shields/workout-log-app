namespace WorkoutLog.UI.Components.Inputs;

/// <summary>One choice in a <see cref="RadioGroup{TValue}"/>.</summary>
/// <param name="Value">Value chosen.</param>
/// <param name="Label">Text of the choice.</param>
/// <param name="TestId">Value of the radio's data-testid attribute.</param>
/// <param name="Hint">Smaller text under the label.</param>
public sealed record RadioOption<TValue>(TValue Value, string Label, string TestId, string? Hint = null);
