namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Picks an assisted machine's records from its sessions.</summary>
internal interface IAssistedRecordsRule
{
    /// <summary>The records of <paramref name="sessions"/>.</summary>
    AssistedRecords Records(IReadOnlyList<AssistedSessionProgress> sessions);
}
