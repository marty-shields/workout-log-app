using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>The exercise list, read from the embedded exercises.json.</summary>
public sealed class ExerciseCatalog : IExerciseCatalog
{
    private const string ResourceName = "WorkoutLog.Core.exercises.json";

    /// <summary>Options for reading the file: enum names only, unknown fields and missing required fields fail.</summary>
    internal static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) },
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
    };

    /// <summary>Reads the embedded file. Throws if the resource is missing or doesn't match <see cref="ExerciseDefinition"/>.</summary>
    public ExerciseCatalog()
    {
        using var stream = typeof(ExerciseCatalog).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource {ResourceName} not found");
        var exercises = JsonSerializer.Deserialize<List<ExerciseDefinition>>(stream, Options)
            ?? throw new JsonException($"{ResourceName} is empty");
        All = exercises.OrderBy(e => e.Id).ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<ExerciseDefinition> All { get; }
}
