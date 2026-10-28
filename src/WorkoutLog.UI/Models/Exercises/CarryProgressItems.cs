using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Components.Charts;
using WorkoutLog.UI.Components.Inputs;
using WorkoutLog.UI.Components.Lists;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Models.Exercises;

/// <summary>
/// Builds the text of a carry's Exercise page from its <see cref="CarryProgress"/> (FR-63). Distances are in the exercise's unit.
/// A weight of 0kg or blank isn't shown.
/// </summary>
public static class CarryProgressItems
{
    /// <summary>Key of the Weight tab.</summary>
    public const string WeightTab = "weight";

    /// <summary>Key of the Distance tab.</summary>
    public const string DistanceTab = "distance";

    /// <summary>The carry tabs: Weight, Distance.</summary>
    public static IReadOnlyList<RadioOption<string>> Tabs { get; } =
        [new(WeightTab, "Weight", $"exercise-tab-{WeightTab}"), new(DistanceTab, "Distance", $"exercise-tab-{DistanceTab}")];

    /// <summary>A distance in the exercise's unit, e.g. 40m.</summary>
    public static Func<decimal, string> Distance(ExerciseDefinition exercise) =>
        value => $"{ExerciseProgressItems.Number(value)}{ExerciseFields.DistanceText(exercise)}";

    /// <summary>
    /// One point per session with a value for <paramref name="tab"/>, oldest first: the heaviest weight labelled with its distance,
    /// or the longest distance labelled with its weight.
    /// </summary>
    public static IReadOnlyList<ChartPoint> Points(CarryProgress progress, string tab, ExerciseDefinition exercise, TimeZoneInfo zone, DateTimeOffset now)
    {
        var distance = Distance(exercise);
        return progress.Sessions
            .Select(s => tab == DistanceTab
                ? s.Longest is { Distance: { } metres } longest
                    ? new ChartPoint(ExerciseProgressItems.Day(s.Session.StartedAt, zone, now), metres, longest.WeightKg is { } weight ? DisplayFormat.Weight(weight) : null)
                    : null
                : s.Heaviest is { WeightKg: { } kg } heaviest
                    ? new ChartPoint(ExerciseProgressItems.Day(s.Session.StartedAt, zone, now), kg, heaviest.Distance is { } d ? distance(d) : null)
                    : null)
            .OfType<ChartPoint>()
            .ToList();
    }

    /// <summary>No sets in the range text when sessions are in the range but none has a value for <paramref name="tab"/>; otherwise null.</summary>
    public static string? NoDataMessage(CarryProgress progress, string tab, ChartRange range) =>
        progress.Sessions.Count > 0 && progress.Sessions.All(s => tab == DistanceTab ? s.Longest is null : s.Heaviest is null)
            ? ExerciseProgressItems.NoSets(range)
            : null;

    /// <summary>Heaviest weight (e.g. 32kg · 40m) and Longest distance (e.g. 60m · 24kg, or 60m), each with its date; no value shows —.</summary>
    public static IReadOnlyList<RecordRow> Records(CarryProgress progress, ExerciseDefinition exercise, TimeZoneInfo zone, DateTimeOffset now) =>
    [
        progress.Records.HeaviestWeight is { } heaviest
            ? new RecordRow("Heaviest weight", Heaviest(heaviest.Value, exercise), ExerciseProgressItems.Day(heaviest.StartedAt, zone, now))
            : new RecordRow("Heaviest weight", null, null),
        progress.Records.LongestDistance is { } longest
            ? new RecordRow("Longest distance", Longest(longest.Value, exercise), ExerciseProgressItems.Day(longest.StartedAt, zone, now))
            : new RecordRow("Longest distance", null, null),
    ];

    /// <summary>
    /// Each session newest first: its date, its heaviest set (e.g. 32kg · 40m) or, with no weighted set, its longest (e.g. 60m), and every set
    /// in order (e.g. 28kg 40m 0:45 · 60m).
    /// </summary>
    public static IReadOnlyList<ExerciseSession> Sessions(CarryProgress progress, ExerciseDefinition exercise, TimeZoneInfo zone, DateTimeOffset now) =>
        progress.Sessions
            .OrderByDescending(s => s.Session.StartedAt)
            .Select(s => new ExerciseSession(
                s.Session.WorkoutId,
                DisplayFormat.ShortDate(ExerciseProgressItems.Local(s.Session.StartedAt, zone), ExerciseProgressItems.Local(now, zone)),
                s.Heaviest is { } heaviest ? Heaviest(heaviest, exercise) : s.Longest is { } longest ? Longest(longest, exercise) : "—",
                string.Join(" · ", s.Session.Sets.Select(set => SetText(set, exercise)))))
            .ToList();

    private static string Heaviest(ExerciseBest best, ExerciseDefinition exercise) =>
        Join(" · ", DisplayFormat.Weight(best.WeightKg), best.Distance is { } d ? Distance(exercise)(d) : null);

    private static string Longest(ExerciseBest best, ExerciseDefinition exercise) =>
        Join(" · ", best.Distance is { } d ? Distance(exercise)(d) : null, best.WeightKg is { } kg ? DisplayFormat.Weight(kg) : null);

    private static string SetText(StrengthSet set, ExerciseDefinition exercise)
    {
        var text = Join(" ", set.WeightKg > 0m ? DisplayFormat.Weight(set.WeightKg) : null, set.Distance is { } d ? Distance(exercise)(d) : null,
            set.Duration is { } duration ? DisplayFormat.Clock(duration) : null);
        return text.Length > 0 ? text : "—";
    }

    private static string Join(string separator, params string?[] parts) => string.Join(separator, parts.OfType<string>());
}
