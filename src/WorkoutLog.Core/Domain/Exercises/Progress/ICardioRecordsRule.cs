namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Picks a cardio exercise's records from its sessions.</summary>
internal interface ICardioRecordsRule
{
    /// <summary>The records of <paramref name="sessions"/>.</summary>
    CardioRecords Records(IReadOnlyList<CardioSessionProgress> sessions);
}
