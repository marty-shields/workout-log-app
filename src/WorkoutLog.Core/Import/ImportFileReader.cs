using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using WorkoutLog.Core.Export;
using WorkoutLog.Core.Import.Upgrades;
using WorkoutLog.Core.Services;

namespace WorkoutLog.Core.Import;

/// <summary>Reads the file into a JSON tree, applies the format upgrade steps after its version, then reads each workout on its own.</summary>
internal sealed class ImportFileReader : IImportFileReader
{
    private const int ChunkBytes = 81920;

    private readonly IReadOnlyList<IFormatUpgrade> upgrades;
    private readonly int currentVersion;
    private readonly int maxBytes;

    /// <summary>Uses <see cref="ExportFormat.Version"/> and <see cref="ExportFormat.MaxBytes"/>.</summary>
    public ImportFileReader(IEnumerable<IFormatUpgrade> upgrades)
        : this(upgrades, ExportFormat.Version, ExportFormat.MaxBytes)
    {
    }

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> unless <paramref name="upgrades"/> are exactly one step for each version from 2 to <paramref name="currentVersion"/>.
    /// </summary>
    internal ImportFileReader(IEnumerable<IFormatUpgrade> upgrades, int currentVersion, int maxBytes)
    {
        this.upgrades = upgrades.OrderBy(u => u.Version).ToList();
        if (!this.upgrades.Select(u => u.Version).SequenceEqual(Enumerable.Range(2, currentVersion - 1)))
        {
            throw new InvalidOperationException(
                $"Expected one format upgrade step for each version from 2 to {currentVersion}, found [{string.Join(", ", this.upgrades.Select(u => u.Version))}]");
        }

        this.currentVersion = currentVersion;
        this.maxBytes = maxBytes;
    }

    /// <inheritdoc />
    public async Task<ImportFileRead> ReadAsync(Stream stream)
    {
        using var content = new MemoryStream();
        var chunk = new byte[ChunkBytes];
        int read;
        while ((read = await stream.ReadAsync(chunk)) > 0)
        {
            content.Write(chunk, 0, read);
            if (content.Length > maxBytes)
            {
                return new ImportFileRead(ImportError.TooLarge, []);
            }
        }

        content.Position = 0;
        try
        {
            return Read(content);
        }
        catch (Exception exception)
        {
            return new ImportFileRead(ImportError.NotAnExport, [], exception);
        }
    }

    private ImportFileRead Read(Stream content)
    {
        if (JsonNode.Parse(content) is not JsonObject file || Version(file) is not { } version)
        {
            return new ImportFileRead(ImportError.NotAnExport, []);
        }

        if (version > currentVersion)
        {
            return new ImportFileRead(ImportError.NewerVersion, []);
        }

        foreach (var upgrade in upgrades.Where(u => u.Version > version))
        {
            upgrade.Apply(file);
        }

        if (!IsExportedAt(Text(file, "exportedAt")) || string.IsNullOrWhiteSpace(Text(file, "appVersion")) || file["workouts"] is not JsonArray workouts)
        {
            return new ImportFileRead(ImportError.NotAnExport, []);
        }

        return workouts.Count == 0
            ? new ImportFileRead(ImportError.NoWorkouts, [])
            : new ImportFileRead(ImportError.None, workouts.Select((w, i) => ReadWorkout(w, i + 1)).ToList());
    }

    /// <summary>Reads one workout. Its times are kept as text for the checks; anything else that can't be read makes it malformed.</summary>
    private static ImportWorkout ReadWorkout(JsonNode? node, int position)
    {
        if (node is not JsonObject workout)
        {
            return new ImportWorkout(position, null, null, null);
        }

        string? started = null;
        string? finished = null;
        try
        {
            started = Text(workout, "startedAt");
            finished = Text(workout, "finishedAt");
            var body = workout.Deserialize<ImportWorkoutBody>(ImportFormat.Options);
            return body is not null && !body.Exercises.Contains(null!) && body.Exercises.All(e => e.IsWellFormed)
                ? new ImportWorkout(position, started, finished, body.Exercises)
                : new ImportWorkout(position, started, finished, null);
        }
        catch (Exception)
        {
            return new ImportWorkout(position, started, finished, null);
        }
    }

    /// <summary>The format version: a whole JSON number of at least 1, or null.</summary>
    private static int? Version(JsonObject file) =>
        file["formatVersion"] is JsonValue value && value.GetValueKind() == JsonValueKind.Number && value.TryGetValue<int>(out var version) && version >= 1
            ? version
            : null;

    private static bool IsExportedAt(string? text) =>
        DateTimeOffset.TryParseExact(text, "yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _);

    private static string? Text(JsonObject json, string name) =>
        json[name] is JsonValue value && value.GetValueKind() == JsonValueKind.String ? value.GetValue<string>() : null;
}
