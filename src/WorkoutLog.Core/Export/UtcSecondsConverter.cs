using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorkoutLog.Core.Export;

/// <summary>Writes a time as UTC to the second: <c>yyyy-MM-ddTHH:mm:ssZ</c>. Write only.</summary>
internal sealed class UtcSecondsConverter : JsonConverter<DateTimeOffset>
{
    /// <inheritdoc />
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotSupportedException();

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture));
}
