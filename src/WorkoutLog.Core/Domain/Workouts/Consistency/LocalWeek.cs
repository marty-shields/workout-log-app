namespace WorkoutLog.Core.Domain.Workouts.Consistency;

/// <summary>Weeks from Monday 00:00 to Sunday 23:59:59 in a time zone.</summary>
internal static class LocalWeek
{
    /// <summary>Weeks on the Dashboard's bars: this week and the 11 before it.</summary>
    public const int Shown = 12;

    /// <summary>The Monday of the week <paramref name="day"/> is in.</summary>
    public static DateOnly Monday(DateOnly day) => day.AddDays(-(((int)day.DayOfWeek + 6) % 7));

    /// <summary>The Monday of the week <paramref name="instant"/> is in, in <paramref name="zone"/>.</summary>
    public static DateOnly Monday(DateTimeOffset instant, TimeZoneInfo zone) =>
        Monday(DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, zone).DateTime));

    /// <summary>
    /// The instant of <paramref name="local"/> in <paramref name="zone"/>: the end of the gap if it is skipped, the first if it repeats.
    /// </summary>
    public static DateTimeOffset Instant(DateTime local, TimeZoneInfo zone)
    {
        while (zone.IsInvalidTime(local))
        {
            local = local.AddMinutes(1);
        }

        var offset = zone.IsAmbiguousTime(local) ? zone.GetAmbiguousTimeOffsets(local).Max() : zone.GetUtcOffset(local);
        return new DateTimeOffset(local, offset);
    }

    /// <summary>The first instant of <paramref name="day"/> in <paramref name="zone"/>.</summary>
    public static DateTimeOffset Start(DateOnly day, TimeZoneInfo zone) => Instant(day.ToDateTime(TimeOnly.MinValue), zone);
}
