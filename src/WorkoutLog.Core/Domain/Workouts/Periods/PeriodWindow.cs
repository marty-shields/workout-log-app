using WorkoutLog.Core.Domain.Workouts.Consistency;

namespace WorkoutLog.Core.Domain.Workouts.Periods;

/// <summary>Which local days a period and the one before it cover when they are compared.</summary>
internal readonly record struct PeriodWindow(DateOnly From, DateOnly To, DateOnly EarlierFrom, DateOnly EarlierTo)
{
    /// <summary>A period that holds <paramref name="today"/> is cut at today, the earlier one at the same point; any other period is compared whole to whole.</summary>
    public static PeriodWindow Of(Period period, DateOnly today)
    {
        var earlier = period.Previous();
        return period.Contains(today)
            ? new PeriodWindow(period.Start, today, earlier.Start, period.SamePointBefore(today))
            : new PeriodWindow(period.Start, period.End, earlier.Start, earlier.End);
    }

    /// <summary>Whether the local start date of <paramref name="startedAt"/> lies in <paramref name="from"/> to <paramref name="to"/>, both included.</summary>
    public static bool IsOn(DateTimeOffset startedAt, DateOnly from, DateOnly to, TimeZoneInfo zone)
    {
        var day = DateOnly.FromDateTime(LocalWeek.Local(startedAt, zone));
        return day >= from && day <= to;
    }
}
