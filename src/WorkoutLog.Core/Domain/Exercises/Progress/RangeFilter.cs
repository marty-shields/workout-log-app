namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>
/// Calendar months in the phone's time zone: a session is in range when its local start date is on or after today minus 3, 6 or 12 months.
/// <see cref="ChartRange.All"/> includes every session.
/// </summary>
internal sealed class RangeFilter(TimeProvider clock) : IRangeFilter
{
    /// <inheritdoc />
    public bool Includes(DateTimeOffset startedAt, ChartRange range)
    {
        if (range == ChartRange.All)
        {
            return true;
        }

        var zone = clock.LocalTimeZone;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), zone).DateTime);
        var from = today.AddMonths(range switch
        {
            ChartRange.ThreeMonths => -3,
            ChartRange.SixMonths => -6,
            _ => -12,
        });
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(startedAt, zone).DateTime) >= from;
    }
}
