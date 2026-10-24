using System.Globalization;
using System.Text.RegularExpressions;

namespace WorkoutLog.E2E.Tests.Seeding;

/// <summary>
/// Reads a time as steps write it, against the device's local now: "3 hours ago", "yesterday 18:05",
/// "10 days ago 09:00" or "tomorrow 09:00". "N hours ago" must fall on today's date.
/// </summary>
public static partial class SeedTime
{
    public static DateTime Resolve(string text, DateTime now)
    {
        if (HoursAgo().Match(text) is { Success: true } hours)
        {
            var time = now.AddHours(-int.Parse(hours.Groups[1].Value, CultureInfo.InvariantCulture));
            if (time.Date != now.Date)
            {
                throw new InvalidOperationException($"\"{text}\" isn't today at {now:HH:mm} on the device: run this scenario later in the day.");
            }

            return time;
        }

        if (DayAndTime().Match(text) is not { Success: true } match)
        {
            throw new FormatException($"Can't read the time \"{text}\".");
        }

        var day = match.Groups["day"].Value;
        var offset = day switch
        {
            "today" => 0,
            "yesterday" => -1,
            "tomorrow" => 1,
            _ => -int.Parse(day.Split(' ')[0], CultureInfo.InvariantCulture),
        };

        var clock = TimeSpan.ParseExact(match.Groups["time"].Value, @"hh\:mm", CultureInfo.InvariantCulture);
        return now.Date.AddDays(offset) + clock;
    }

    /// <summary>The day only, e.g. "3 days ago" or "yesterday".</summary>
    public static DateTime ResolveDay(string text, DateTime now) =>
        HoursAgo().IsMatch(text) ? Resolve(text, now).Date : Resolve($"{text} 12:00", now).Date;

    [GeneratedRegex(@"^(\d+) hours? ago$")]
    private static partial Regex HoursAgo();

    [GeneratedRegex(@"^(?<day>today|yesterday|tomorrow|\d+ days? ago) (?<time>\d{2}:\d{2})$")]
    private static partial Regex DayAndTime();
}
