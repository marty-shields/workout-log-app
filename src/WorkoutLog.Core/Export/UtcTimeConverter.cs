using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorkoutLog.Core.Export;

/// <summary>Writes a time as UTC with 7 fractional digits: <c>yyyy-MM-ddTHH:mm:ss.fffffffZ</c>. Write only.</summary>
internal sealed class UtcTimeConverter : JsonConverter<DateTimeOffset>
{
    /// <inheritdoc />
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotSupportedException();

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture));
}
