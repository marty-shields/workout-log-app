namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Picks a hold's records from its sessions.</summary>
internal interface IHoldRecordsRule
{
    /// <summary>The records of <paramref name="sessions"/>.</summary>
    HoldRecords Records(IReadOnlyList<HoldSessionProgress> sessions);
}
