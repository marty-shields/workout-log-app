using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Services;

/// <summary>A muscle group's strength sets in the last 7 days.</summary>
/// <param name="Group">Muscle group.</param>
/// <param name="Sets">Sets of exercises whose primary muscle is in the group; 0 when not trained.</param>
public sealed record MuscleGroupCount(MuscleGroup Group, int Sets);
