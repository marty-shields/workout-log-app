namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A carry's sessions and records in a range.</summary>
/// <param name="Sessions">Sessions in the range, by start time oldest first.</param>
/// <param name="Records">Records of those sessions.</param>
public sealed record CarryProgress(IReadOnlyList<CarrySessionProgress> Sessions, CarryRecords Records);
