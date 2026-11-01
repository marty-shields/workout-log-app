namespace WorkoutLog.Core.Services;

/// <summary>Outcome of loading the sets per muscle group.</summary>
/// <param name="Groups">Every muscle group in order, with its sets. Empty when loading failed.</param>
/// <param name="Failed">True when the sets couldn't be read.</param>
public sealed record MuscleSetsResult(IReadOnlyList<MuscleGroupCount> Groups, bool Failed);
