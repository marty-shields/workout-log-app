using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Export;

/// <summary>Writes a decimal as a JSON number rounded by <see cref="Decimals.Round2"/>, without trailing zeros. Reads a JSON number as it is.</summary>
internal sealed class DecimalConverter : JsonConverter<decimal>
{
    /// <inheritdoc />
    /// <remarks>Throws <see cref="JsonException"/> on a string or a number out of the decimal range.</remarks>
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.Number && reader.TryGetDecimal(out var value)
            ? value
            : throw new JsonException("Expected a number");

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) =>
        writer.WriteRawValue(Decimals.Round2(value).ToString("0.##", CultureInfo.InvariantCulture));
}
