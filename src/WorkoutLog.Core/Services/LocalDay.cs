namespace WorkoutLog.Core.Services;

/// <summary>Instants of local calendar dates.</summary>
internal static class LocalDay
{
    /// <summary>First instant of the date in the zone: the end of the gap if midnight is skipped, the first midnight if it repeats.</summary>
    public static DateTimeOffset StartOf(DateTime date, TimeZoneInfo zone)
    {
        var local = date;
        while (zone.IsInvalidTime(local))
        {
            local = local.AddMinutes(1);
        }

        var offset = zone.IsAmbiguousTime(local) ? zone.GetAmbiguousTimeOffsets(local).Max() : zone.GetUtcOffset(local);
        return new DateTimeOffset(local, offset);
    }
}
