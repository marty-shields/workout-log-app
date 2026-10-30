namespace WorkoutLog.Core.Services;

/// <summary>Reads the records set recently, for the Dashboard.</summary>
public interface IRecentRecordsService
{
    /// <summary>
    /// Returns up to 3 records from sessions started today or in the 29 days before it, in the phone's time zone, and not after now:
    /// the latest per exercise, newest first, exercises from the same workout A–Z. Ids not in the exercise list are left out.
    /// A failed read returns <see cref="RecentRecordsResult.Failed"/> and no records.
    /// </summary>
    Task<RecentRecordsResult> GetAsync();
}
