namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Picks a carry's records from its sessions.</summary>
internal interface ICarryRecordsRule
{
    /// <summary>The records of <paramref name="sessions"/>.</summary>
    CarryRecords Records(IReadOnlyList<CarrySessionProgress> sessions);
}
