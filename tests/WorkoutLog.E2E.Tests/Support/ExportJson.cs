using System.Globalization;
using System.Text.Json.Nodes;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.E2E.Tests.Support;

/// <summary>Workouts as the export file writes them (docs/02 Export file).</summary>
public static class ExportJson
{
    /// <summary>A file with the version 2 header and <paramref name="workouts"/>.</summary>
    public static JsonObject File(IEnumerable<JsonNode> workouts) => new()
    {
        ["formatVersion"] = 2,
        ["exportedAt"] = "2026-10-26T18:05:12Z",
        ["appVersion"] = "1.0.0",
        ["workouts"] = new JsonArray(workouts.ToArray()),
    };

    /// <summary>A workout: start and finish as UTC with 7 fractional digits, then its exercises.</summary>
    public static JsonObject Workout(DateTimeOffset started, DateTimeOffset finished, IEnumerable<ExerciseValues> exercises) => new()
    {
        ["startedAt"] = Time(started),
        ["finishedAt"] = Time(finished),
        ["exercises"] = new JsonArray(exercises.Select(Exercise).ToArray()),
    };

    public static string Time(DateTimeOffset time) =>
        time.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture);

    /// <summary>An exercise: id, name, type and units, then its sets or entry.</summary>
    public static JsonNode Exercise(ExerciseValues values)
    {
        var definition = values.Definition;
        var exercise = new JsonObject { ["id"] = definition.Id, ["name"] = definition.Name, ["type"] = definition.Type.ToString() };
        AddIf(exercise, "distanceUnit", definition.DistanceUnit?.ToString());
        AddIf(exercise, "rateUnit", definition.RateUnit?.ToString());
        AddIf(exercise, "paceUnit", definition.PaceUnit?.ToString());
        var node = new JsonObject { ["exercise"] = exercise };
        switch (values.ToWorkoutExercise())
        {
            case StrengthExercise strength:
                node["sets"] = new JsonArray(strength.Sets.Select(s => (JsonNode)Values(
                    ("reps", s.Reps), ("weightKg", s.WeightKg), ("duration", s.Duration), ("distance", s.Distance))).ToArray());
                break;
            case CardioExercise { Entry: var e }:
                node["entry"] = Values(("duration", e.Duration), ("distance", e.Distance), ("floors", e.Floors), ("level", e.Level),
                    ("incline", e.Incline), ("calories", e.Calories), ("rate", e.Rate));
                break;
        }

        return node;
    }

    /// <summary>The values that are set, in order: numbers as JSON numbers, times as hh:mm:ss.</summary>
    private static JsonObject Values(params (string Key, object? Value)[] values)
    {
        var node = new JsonObject();
        foreach (var (key, value) in values)
        {
            switch (value)
            {
                case int whole:
                    node[key] = whole;
                    break;
                case decimal number:
                    node[key] = JsonNode.Parse(number.ToString("0.##", CultureInfo.InvariantCulture));
                    break;
                case TimeSpan time:
                    node[key] = time.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
                    break;
            }
        }

        return node;
    }

    private static void AddIf(JsonObject node, string key, string? value)
    {
        if (value is not null)
        {
            node[key] = value;
        }
    }
}
