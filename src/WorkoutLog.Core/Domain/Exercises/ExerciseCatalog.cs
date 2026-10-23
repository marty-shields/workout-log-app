using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>The exercise list, read from the embedded exercises.json on first use.</summary>
public sealed class ExerciseCatalog : IExerciseCatalog
{
    /// <summary>Longest query <see cref="Search"/> reads; longer text is cut to this length.</summary>
    public const int MaxQueryLength = 50;

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
    private readonly Lazy<IReadOnlyList<Entry>> entries;

    /// <summary>Uses the embedded exercises.json.</summary>
    public ExerciseCatalog()
        : this(ResourceName)
    {
    }

    internal ExerciseCatalog(string resourceName)
    {
        all = new(() => Load(resourceName));
        entries = new(() => all.Value
            .Select(e => new Entry(e, SearchText.Words(e.Name), e.Aliases.SelectMany(SearchText.Words).ToArray()))
            .OrderBy(e => e.Exercise.Name, StringComparer.OrdinalIgnoreCase)
            .ToList());
    }

    /// <inheritdoc />
    /// <remarks>Throws if the resource is missing or doesn't match <see cref="ExerciseDefinition"/>.</remarks>
    public IReadOnlyList<ExerciseDefinition> All => all.Value;

    /// <inheritdoc />
    public IReadOnlyList<ExerciseDefinition> Search(string? query)
    {
        var words = SearchText.Words(query is { Length: > MaxQueryLength } ? query[..MaxQueryLength] : query);
        if (words.Length == 0)
        {
            return entries.Value.OrderBy(e => e.Exercise.Type).Select(e => e.Exercise).ToList();
        }

        return entries.Value
            .Select(e => (e.Exercise, Rank: Rank(e, words)))
            .Where(r => r.Rank is not null)
            .OrderBy(r => r.Rank)
            .Select(r => r.Exercise)
            .ToList();
    }

    /// <summary>0: each query word, in order, starts the name's word in the same place; 1: other name match; 2: match using nicknames; null: no match.</summary>
    private static int? Rank(Entry entry, string[] words)
    {
        if (!words.All(w => HasPrefix(entry.NameWords, w) || HasPrefix(entry.AliasWords, w)))
        {
            return null;
        }

        if (!words.All(w => HasPrefix(entry.NameWords, w)))
        {
            return 2;
        }

        return StartsName(entry.NameWords, words) ? 0 : 1;
    }

    private static bool StartsName(string[] name, string[] words) =>
        words.Length <= name.Length && words.Select((w, i) => name[i].StartsWith(w, StringComparison.Ordinal)).All(match => match);

    private static bool HasPrefix(string[] words, string prefix) => words.Any(w => w.StartsWith(prefix, StringComparison.Ordinal));

    private static List<ExerciseDefinition> Load(string resourceName)
    {
        using var stream = typeof(ExerciseCatalog).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource {resourceName} not found");
        var exercises = JsonSerializer.Deserialize<List<ExerciseDefinition>>(stream, Options)
            ?? throw new JsonException($"{resourceName} is empty");
        return exercises.OrderBy(e => e.Id).ToList();
    }

    /// <summary>An exercise with its normalised name and nickname words, for search.</summary>
    private sealed record Entry(ExerciseDefinition Exercise, string[] NameWords, string[] AliasWords);
}
