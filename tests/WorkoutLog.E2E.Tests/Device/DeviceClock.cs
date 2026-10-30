namespace WorkoutLog.E2E.Tests.Device;

/// <summary>The device's clock and time zone, read over adb.</summary>
public sealed class DeviceClock
{
    private TimeZoneInfo? zone;

    /// <summary>The device's IANA time zone.</summary>
    public TimeZoneInfo Zone => zone ??= TimeZoneInfo.FindSystemTimeZoneById(AppiumSetup.Adb.Shell("getprop", "persist.sys.timezone"));

    public DateTimeOffset UtcNow() => DateTimeOffset.FromUnixTimeSeconds(long.Parse(AppiumSetup.Adb.Shell("date", "+%s")));

    public DateTime LocalNow() => ToLocal(UtcNow());

    public DateTime ToLocal(DateTimeOffset time) => TimeZoneInfo.ConvertTime(time, Zone).DateTime;

    public DateTimeOffset FromLocal(DateTime local) => new(local, Zone.GetUtcOffset(local));
}
