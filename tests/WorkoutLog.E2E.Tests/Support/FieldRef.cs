using System.Text.RegularExpressions;

namespace WorkoutLog.E2E.Tests.Support;

/// <summary>
/// A field as steps name it: "Barbell Back Squat set 1 weight", "Treadmill distance" or "Plank set 1 duration minutes".
/// <see cref="Set"/> is null for a cardio entry; <see cref="Box"/> is a duration or pace box (h, mm, ss or m, ss), or null.
/// </summary>
internal sealed partial record FieldRef(string Name, int? Set, string Key, string? Box)
{
    public static FieldRef Parse(string text)
    {
        var match = Pattern().Match(text);
        if (!match.Success)
        {
            throw new FormatException($"Can't read the field \"{text}\".");
        }

        var key = match.Groups["key"].Value;
        var box = match.Groups["part"].Value switch
        {
            "" => null,
            "hours" => "h",
            "minutes" => key == "pace" ? "m" : "mm",
            _ => "ss",
        };

        return new FieldRef(match.Groups["name"].Value, match.Groups["set"].Success ? int.Parse(match.Groups["set"].Value) : null, key, box);
    }

    public static bool TryParse(string text, out FieldRef? field)
    {
        field = Pattern().IsMatch(text) ? Parse(text) : null;
        return field is not null;
    }

    [GeneratedRegex(@"^(?<name>.+?)(?: set (?<set>\d+))? (?<key>reps|weight|duration|distance|incline|level|calories|pace|floors|rate)(?: (?<part>hours|minutes|seconds))?$")]
    private static partial Regex Pattern();
}
