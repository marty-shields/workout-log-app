namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A bodyweight move's sessions and records in a range.</summary>
/// <param name="Sessions">Sessions in the range, by start time oldest first.</param>
/// <param name="Records">Records of those sessions.</param>
public sealed record BodyweightProgress(IReadOnlyList<BodyweightSessionProgress> Sessions, BodyweightRecords Records);
