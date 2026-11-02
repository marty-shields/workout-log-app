using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace WorkoutLog.Core.Export;

/// <summary>Writes and reads a duration as <c>hh:mm:ss</c>. Hours have 2 or more digits.</summary>
internal sealed partial class TimeConverter : JsonConverter<TimeSpan>
{
    /// <summary>Most hours that leave room for any minutes and seconds in a <see cref="TimeSpan"/>.</summary>
    private static readonly long MaxHours = (long)TimeSpan.MaxValue.TotalHours - 1;

    /// <inheritdoc />
    /// <remarks>Throws <see cref="JsonException"/> on anything else, including minutes or seconds over 59 and hours past <see cref="TimeSpan"/>'s range.</remarks>
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.String
        && Pattern().Match(reader.GetString()!) is { Success: true } match
        && long.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var hours)
        && hours <= MaxHours
            ? new TimeSpan((int)hours, int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture), int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture))
            : throw new JsonException("Expected a time as hh:mm:ss");

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) =>
        writer.WriteStringValue(string.Create(CultureInfo.InvariantCulture, $"{(int)value.TotalHours:00}:{value.Minutes:00}:{value.Seconds:00}"));

    [GeneratedRegex(@"^([0-9]{2,})\:([0-5][0-9])\:([0-5][0-9])\z")]
    private static partial Regex Pattern();
}
