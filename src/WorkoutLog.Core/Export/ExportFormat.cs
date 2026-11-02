using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorkoutLog.Core.Export;

/// <summary>The export file format.</summary>
internal static class ExportFormat
{
    /// <summary>Format version written to the file.</summary>
    public const int Version = 2;

    /// <summary>Largest file allowed, in bytes of UTF-8: 5 MB.</summary>
    public const int MaxBytes = 5 * 1024 * 1024;

    /// <summary>Matches every export file name, whatever its date; the one place that knows the name's shape.</summary>
    public const string FilePattern = "workout-log-*.json";

    /// <summary>The name of the export file written at <paramref name="localTime"/>.</summary>
    public static string FileName(DateTimeOffset localTime) =>
        $"workout-log-{localTime.ToString("yyyy-MM-dd-HHmm", CultureInfo.InvariantCulture)}.json";

    /// <summary>Options for writing the file: camelCase, enum names, nulls left out, no indentation.</summary>
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters =
        {
            new JsonStringEnumConverter(allowIntegerValues: false),
            new UtcTimeConverter(),
            new TimeConverter(),
            new DecimalConverter(),
        },
    };
}
