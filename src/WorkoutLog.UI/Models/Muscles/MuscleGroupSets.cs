namespace WorkoutLog.UI.Models.Muscles;

/// <summary>A muscle group's strength sets in the last 7 days.</summary>
/// <param name="Group">Muscle group, e.g. Chest.</param>
/// <param name="Sets">Sets whose exercise's primary muscle is in the group.</param>
public sealed record MuscleGroupSets(string Group, int Sets);
