namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>
/// Calendar months in the phone's time zone: a session is in range when its local start date is on or after today minus 3, 6 or 12 months.
/// <see cref="HistoryRange.All"/> includes every session.
/// </summary>
internal sealed class RangeFilter(TimeProvider clock)
{
    /// <summary>True when the session starting at <paramref name="startedAt"/> is in <paramref name="range"/>.</summary>
    public bool Includes(DateTimeOffset startedAt, HistoryRange range)
    {
        if (range == HistoryRange.All)
        {
            return true;
        }

        var zone = clock.LocalTimeZone;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), zone).DateTime);
        var from = today.AddMonths(range switch
        {
            HistoryRange.ThreeMonths => -3,
            HistoryRange.SixMonths => -6,
            _ => -12,
        });
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(startedAt, zone).DateTime) >= from;
    }
}
