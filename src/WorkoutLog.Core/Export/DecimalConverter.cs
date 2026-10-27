using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Export;

/// <summary>Writes a decimal as a JSON number rounded by <see cref="Decimals.Round2"/>, without trailing zeros. Write only.</summary>
internal sealed class DecimalConverter : JsonConverter<decimal>
{
    /// <inheritdoc />
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotSupportedException();

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) =>
        writer.WriteRawValue(Decimals.Round2(value).ToString("0.##", CultureInfo.InvariantCulture));
}
