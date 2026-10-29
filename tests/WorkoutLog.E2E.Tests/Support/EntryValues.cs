using System.Globalization;
using System.Text.RegularExpressions;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.E2E.Tests.Support;

/// <summary>
/// The values of one set or entry as steps write them, e.g. "8 reps, 60 kg", "40 m 32 kg 0:00:45" or
/// "0:10:00, 2000 m, damper 5, 120 calories", keyed by the app's field keys and held as typed.
/// </summary>
public sealed partial class EntryValues
{
    private static readonly Dictionary<string, string> Units = new()
    {
        ["reps"] = "reps",
        ["kg"] = "weight",
        ["m"] = "distance",
        ["km"] = "distance",
        ["ft"] = "distance",
        ["calories"] = "calories",
        ["floors"] = "floors",
        ["km/h"] = "rate",
        ["rpm"] = "rate",
        ["strokes/min"] = "rate",
        ["strides/min"] = "rate",
        ["steps/min"] = "rate",
        ["feet/min"] = "rate",
    };

    private static readonly Dictionary<string, string> Names = new()
    {
        ["incline"] = "incline",
        ["damper"] = "level",
        ["resistance"] = "level",
        ["level"] = "level",
        ["rate"] = "rate",
    };

    public EntryValues(IReadOnlyDictionary<string, string> values) => Values = values;

    /// <summary>Text of each field, by field key.</summary>
    public IReadOnlyDictionary<string, string> Values { get; }

    public static EntryValues Parse(string text)
    {
        var tokens = text.Replace(",", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var values = new Dictionary<string, string>();
        for (var i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i];
            if (Duration().IsMatch(token))
            {
                values["duration"] = token;
            }
            else if (Names.TryGetValue(token, out var named) && i + 1 < tokens.Length)
            {
                values[named] = tokens[++i];
            }
            else if (i + 1 < tokens.Length && Units.TryGetValue(tokens[i + 1], out var unit))
            {
                values[unit] = token;
                i++;
            }
            else
            {
                throw new FormatException($"Can't read \"{token}\" in \"{text}\".");
            }
        }

        return new EntryValues(values);
    }

    /// <summary>Sets separated by semicolons, e.g. "5 reps 60 kg; 5 reps 62.5 kg".</summary>
    public static IReadOnlyList<EntryValues> ParseSets(string text) =>
        text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(Parse).ToList();

    public StrengthSet ToSet() => new(Int("reps"), Decimal("weight"), Time("duration"), Decimal("distance"));

    public CardioEntry ToEntry() =>
        new(Time("duration"), Decimal("distance"), Int("floors"), Int("level"), Decimal("incline"), Int("calories"), Decimal("rate"));

    private int? Int(string key) => Values.TryGetValue(key, out var text) ? int.Parse(text, CultureInfo.InvariantCulture) : null;

    private decimal? Decimal(string key) => Values.TryGetValue(key, out var text) ? decimal.Parse(text, CultureInfo.InvariantCulture) : null;

    private TimeSpan? Time(string key) => Values.TryGetValue(key, out var text) ? TimeSpan.ParseExact(text, @"h\:mm\:ss", CultureInfo.InvariantCulture) : null;

    [GeneratedRegex(@"^\d+:\d{2}:\d{2}$")]
    private static partial Regex Duration();
}
