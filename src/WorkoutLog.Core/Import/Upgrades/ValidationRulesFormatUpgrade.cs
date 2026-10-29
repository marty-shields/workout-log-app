using System.Text.Json;
using System.Text.Json.Nodes;
using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Import.Upgrades;

/// <summary>
/// Format upgrade step 2: a strength set with no <c>weightKg</c> gets 0; <c>calories</c>, <c>level</c>, <c>rate</c> and <c>floors</c> of 0 are removed;
/// <c>pace</c> is removed from exercises with worked-out pace, and <c>rate</c> from exercises with worked-out speed (<see cref="WorkedOut"/>).
/// Parts of the file that don't have the export's shape are left for the reader.
/// </summary>
internal sealed class ValidationRulesFormatUpgrade(IExerciseCatalog catalog) : IFormatUpgrade
{
    private static readonly string[] PositiveOnly = ["calories", "level", "rate", "floors"];

    private readonly Dictionary<int, ExerciseDefinition> definitions = catalog.All.ToDictionary(e => e.Id);

    /// <inheritdoc />
    public int Version => 2;

    /// <inheritdoc />
    public void Apply(JsonObject file)
    {
        if (file["workouts"] is not JsonArray workouts)
        {
            return;
        }

        foreach (var exercise in workouts.OfType<JsonObject>().Select(w => w["exercises"]).OfType<JsonArray>().SelectMany(e => e.OfType<JsonObject>()))
        {
            if (exercise["sets"] is JsonArray sets)
            {
                foreach (var set in sets.OfType<JsonObject>().Where(s => s["weightKg"] is null))
                {
                    set["weightKg"] = 0;
                }
            }

            if (exercise["entry"] is JsonObject entry)
            {
                Upgrade(entry, Definition(exercise));
            }
        }
    }

    private static void Upgrade(JsonObject entry, ExerciseDefinition? definition)
    {
        foreach (var name in PositiveOnly.Where(n => IsZero(entry[n])))
        {
            entry.Remove(name);
        }

        if (definition is not null && WorkedOut.HasPace(definition))
        {
            entry.Remove("pace");
        }

        if (definition is not null && WorkedOut.HasSpeed(definition))
        {
            entry.Remove("rate");
        }
    }

    private ExerciseDefinition? Definition(JsonObject exercise) =>
        exercise["exercise"] is JsonObject info && info["id"] is JsonValue id && id.GetValueKind() == JsonValueKind.Number
            && id.TryGetValue<int>(out var exerciseId)
            ? definitions.GetValueOrDefault(exerciseId)
            : null;

    private static bool IsZero(JsonNode? node) =>
        node is JsonValue value && value.GetValueKind() == JsonValueKind.Number && value.TryGetValue<decimal>(out var number) && number == 0m;
}
