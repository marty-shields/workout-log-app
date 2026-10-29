namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Picks a lift's records from its sessions.</summary>
internal interface ILiftRecordsRule
{
    /// <summary>The records of <paramref name="sessions"/>.</summary>
    LiftRecords Records(IReadOnlyList<LiftSessionProgress> sessions);
}
