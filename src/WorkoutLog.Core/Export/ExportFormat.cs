using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorkoutLog.Core.Export;

/// <summary>The export file format (docs/02 Export file).</summary>
internal static class ExportFormat
{
    /// <summary>Format version written to the file.</summary>
    public const int Version = 1;

    /// <summary>Largest file allowed, in bytes of UTF-8: 5 MB.</summary>
    public const int MaxBytes = 5 * 1024 * 1024;

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
