namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Picks a bodyweight move's records from its sessions.</summary>
internal interface IBodyweightRecordsRule
{
    /// <summary>The records of <paramref name="sessions"/>.</summary>
    BodyweightRecords Records(IReadOnlyList<BodyweightSessionProgress> sessions);
}
