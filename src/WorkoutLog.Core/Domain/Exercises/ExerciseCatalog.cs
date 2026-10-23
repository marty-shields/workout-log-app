using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>The exercise list, read from the embedded exercises.json on first use.</summary>
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

    private readonly Lazy<IReadOnlyList<ExerciseDefinition>> all;

    /// <summary>Uses the embedded exercises.json.</summary>
    public ExerciseCatalog()
        : this(ResourceName)
    {
    }

    internal ExerciseCatalog(string resourceName) => all = new(() => Load(resourceName));

    /// <inheritdoc />
    /// <remarks>Throws if the resource is missing or doesn't match <see cref="ExerciseDefinition"/>.</remarks>
    public IReadOnlyList<ExerciseDefinition> All => all.Value;

    private static List<ExerciseDefinition> Load(string resourceName)
    {
        using var stream = typeof(ExerciseCatalog).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource {resourceName} not found");
        var exercises = JsonSerializer.Deserialize<List<ExerciseDefinition>>(stream, Options)
            ?? throw new JsonException($"{resourceName} is empty");
        return exercises.OrderBy(e => e.Id).ToList();
    }
}
