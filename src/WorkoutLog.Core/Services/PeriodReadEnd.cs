using WorkoutLog.Core.Domain.Workouts.Consistency;
using WorkoutLog.Core.Domain.Workouts.Periods;

namespace WorkoutLog.Core.Services;

internal static class PeriodReadEnd
{
    /// <summary>The end of the day after the period's last local day, or <paramref name="now"/> if that is earlier; the extra day leaves room for the zone's offsets.</summary>
    public static DateTimeOffset Of(Period period, DateTimeOffset now, TimeZoneInfo zone)
    {
        var end = LocalWeek.Start(period.End.AddDays(2), zone);
        return end < now ? end : now;
    }
}
