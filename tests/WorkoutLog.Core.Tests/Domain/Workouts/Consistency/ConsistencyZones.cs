namespace WorkoutLog.Core.Tests.Domain.Workouts.Consistency;

/// <summary>Time zones for the consistency tests.</summary>
internal static class ConsistencyZones
{
    public static TimeZoneInfo Plus10 { get; } = TimeZoneInfo.CreateCustomTimeZone("UTC+10", TimeSpan.FromHours(10), "UTC+10", "UTC+10");

    /// <summary>UTC+1, and UTC+2 from Sun 25 Oct 2026 02:00 local (clocks forward one hour) to Sun 28 Mar 2027 03:00 local.</summary>
    public static TimeZoneInfo ForwardOn25Oct { get; } = Zone(TimeSpan.FromHours(1),
        TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 25),
        TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 28));

    /// <summary>UTC-3, and UTC-2 from Sun 4 Oct 2026 23:30 local, so Monday 5 Oct starts at 00:30 (02:30 UTC).</summary>
    public static TimeZoneInfo MidnightSkippedOn5Oct { get; } = Zone(TimeSpan.FromHours(-3),
        TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 23, 30, 0), 10, 4),
        TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 2, 0, 0), 2, 15));

    /// <summary>UTC+1, and UTC+2 until Mon 26 Oct 2026 01:00 local, when the clocks go back to 00:00, so midnight is 22:00 and 23:00 UTC on the 25th.</summary>
    public static TimeZoneInfo MidnightTwiceOn26Oct { get; } = Zone(TimeSpan.FromHours(1),
        TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 2, 0, 0), 3, 29),
        TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 1, 0, 0), 10, 26));

    private static TimeZoneInfo Zone(TimeSpan baseOffset, TimeZoneInfo.TransitionTime daylightStart, TimeZoneInfo.TransitionTime daylightEnd) =>
        TimeZoneInfo.CreateCustomTimeZone("Test", baseOffset, "Test", "Test", "Test",
            [TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(DateTime.MinValue.Date, DateTime.MaxValue.Date, TimeSpan.FromHours(1), daylightStart, daylightEnd)]);
}
