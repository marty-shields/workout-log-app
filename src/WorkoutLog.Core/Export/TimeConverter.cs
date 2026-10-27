using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorkoutLog.Core.Export;

/// <summary>Writes a duration or pace as <c>hh:mm:ss</c>. Write only.</summary>
internal sealed class TimeConverter : JsonConverter<TimeSpan>
{
    /// <inheritdoc />
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotSupportedException();

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) =>
        writer.WriteStringValue(string.Create(CultureInfo.InvariantCulture, $"{(int)value.TotalHours:00}:{value.Minutes:00}:{value.Seconds:00}"));
}
