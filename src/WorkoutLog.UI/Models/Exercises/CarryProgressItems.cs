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
/// A weight of 0kg isn't shown.
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
    public static Func<decimal, string> Distance(ExerciseDefinition exercise) => value => ExerciseFields.Distance(exercise, value);

    /// <summary>
    /// One point per session, oldest first: the heaviest weight labelled with its distance, or the longest distance labelled with its weight.
    /// A session with no value for <paramref name="tab"/> has a point with no value.
    /// </summary>
    public static IReadOnlyList<ChartPoint> Points(CarryProgress progress, string tab, ExerciseDefinition exercise, TimeZoneInfo zone, DateTimeOffset now) =>
        progress.Sessions
            .Select(s => tab == DistanceTab
                ? new ChartPoint(ExerciseProgressItems.Day(s.Session.StartedAt, zone, now), s.Longest?.Distance, s.Longest?.WeightKg is { } kg ? DisplayFormat.Weight(kg) : null)
                : new ChartPoint(ExerciseProgressItems.Day(s.Session.StartedAt, zone, now), s.Heaviest?.WeightKg, s.Heaviest?.Distance is { } d ? ExerciseFields.Distance(exercise, d) : null))
            .ToList();

    /// <summary>Heaviest weight (e.g. 32kg · 40m) and Longest distance (e.g. 60m · 24kg, or 60m), each with its date; no value shows —.</summary>
    public static IReadOnlyList<RecordRow> Records(CarryProgress progress, ExerciseDefinition exercise, TimeZoneInfo zone, DateTimeOffset now) =>
    [
        ExerciseProgressItems.Row("Heaviest weight", progress.Records.HeaviestWeight, best => Heaviest(best, exercise), zone, now),
        ExerciseProgressItems.Row("Longest distance", progress.Records.LongestDistance, best => Longest(best, exercise), zone, now),
    ];

    /// <summary>
    /// Each session newest first: its date, its heaviest set (e.g. 32kg · 40m) or, with no weighted set, its longest (e.g. 60m), and every set
    /// in order (e.g. 28kg 40m 0:45 · 60m).
    /// </summary>
    public static IReadOnlyList<ExerciseSession> Sessions(CarryProgress progress, ExerciseDefinition exercise, TimeZoneInfo zone, DateTimeOffset now) =>
        progress.Sessions
            .OrderByDescending(s => s.Session.StartedAt)
            .Select(s => ExerciseProgressItems.Session(
                s.Session,
                s.Heaviest is { } heaviest ? Heaviest(heaviest, exercise) : s.Longest is { } longest ? Longest(longest, exercise) : "—",
                string.Join(" · ", s.Session.Sets.Select(set => SetText(set, exercise))),
                zone,
                now))
            .ToList();

    private static string Heaviest(ExerciseBest best, ExerciseDefinition exercise) =>
        Join(" · ", DisplayFormat.Weight(best.WeightKg), best.Distance is { } d ? ExerciseFields.Distance(exercise, d) : null);

    private static string Longest(ExerciseBest best, ExerciseDefinition exercise) =>
        Join(" · ", best.Distance is { } d ? ExerciseFields.Distance(exercise, d) : null, best.WeightKg is { } kg ? DisplayFormat.Weight(kg) : null);

    private static string SetText(StrengthSet set, ExerciseDefinition exercise)
    {
        var text = Join(" ", set.WeightKg > 0m ? DisplayFormat.Weight(set.WeightKg) : null, set.Distance is { } d ? ExerciseFields.Distance(exercise, d) : null,
            set.Duration is { } duration ? DisplayFormat.Clock(duration) : null);
        return text.Length > 0 ? text : "—";
    }

    private static string Join(string separator, params string?[] parts) => string.Join(separator, parts.OfType<string>());
}
