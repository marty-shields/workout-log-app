using System.Text.Json;
using System.Text.Json.Serialization;
using WorkoutLog.Core.Export;

namespace WorkoutLog.Core.Import;

/// <summary>Reading the export file format (docs/02 Import).</summary>
internal static class ImportFormat
{
    /// <summary>
    /// Options for reading a workout: camelCase names, case-sensitive; enum names only; numbers only as JSON numbers;
    /// required values present and not null.
    /// </summary>
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
        Converters =
        {
            new JsonStringEnumConverter(allowIntegerValues: false),
            new TimeConverter(),
            new DecimalConverter(),
        },
    };
}
