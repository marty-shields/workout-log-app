using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Components.Charts;
using WorkoutLog.UI.Components.Inputs;
using WorkoutLog.UI.Components.Lists;

namespace WorkoutLog.UI.Models.Exercises;

/// <summary>Builds the text of a hold's Exercise page from its <see cref="HoldProgress"/> (FR-63). A weight of 0kg isn't shown.</summary>
public static class HoldProgressItems
{
    /// <summary>Key of the Duration tab.</summary>
    public const string DurationTab = "duration";

    /// <summary>Key of the Weight tab.</summary>
    public const string WeightTab = "weight";

    /// <summary>The hold tabs: Duration, Weight.</summary>
    public static IReadOnlyList<RadioOption<string>> Tabs { get; } =
        [new(DurationTab, "Duration", $"exercise-tab-{DurationTab}"), new(WeightTab, "Weight", $"exercise-tab-{WeightTab}")];

    /// <summary>Seconds on the Duration chart as m:ss, e.g. 1:45.</summary>
    public static readonly Func<decimal, string> Duration = value => DisplayFormat.Clock(TimeSpan.FromSeconds((double)value));

    /// <summary>
    /// One point per session, oldest first: the longest hold in seconds, or the heaviest weight labelled with its duration.
    /// A session with no value for <paramref name="tab"/> has a point with no value.
    /// </summary>
    public static IReadOnlyList<ChartPoint> Points(HoldProgress progress, string tab, TimeZoneInfo zone, DateTimeOffset now) =>
        progress.Sessions
            .Select(s => tab == WeightTab
                ? new ChartPoint(ExerciseProgressItems.Day(s.Session.StartedAt, zone, now), s.Heaviest?.WeightKg, s.Heaviest?.Duration is { } d ? DisplayFormat.Clock(d) : null)
                : new ChartPoint(ExerciseProgressItems.Day(s.Session.StartedAt, zone, now), s.Longest is { } longest ? (decimal)longest.TotalSeconds : null))
            .ToList();

    /// <summary>Longest hold (e.g. 1:45) and Heaviest weight (e.g. 10kg · 1:00), each with its date; no value shows —.</summary>
    public static IReadOnlyList<RecordRow> Records(HoldProgress progress, TimeZoneInfo zone, DateTimeOffset now) =>
    [
        ExerciseProgressItems.Row("Longest hold", progress.Records.LongestHold, DisplayFormat.Clock, zone, now),
        ExerciseProgressItems.Row("Heaviest weight", progress.Records.HeaviestWeight, Heaviest, zone, now),
    ];

    /// <summary>Each session newest first: its date, its longest hold (e.g. 1:45), and every set in order (e.g. 1:45 · 10kg 1:00).</summary>
    public static IReadOnlyList<ExerciseSession> Sessions(HoldProgress progress, TimeZoneInfo zone, DateTimeOffset now) =>
        progress.Sessions
            .OrderByDescending(s => s.Session.StartedAt)
            .Select(s => ExerciseProgressItems.Session(
                s.Session,
                s.Longest is { } longest ? DisplayFormat.Clock(longest) : s.Heaviest is { } heaviest ? Heaviest(heaviest) : "—",
                string.Join(" · ", s.Session.Sets.Select(SetText)),
                zone,
                now))
            .ToList();

    private static string Heaviest(ExerciseBest best) =>
        best.Duration is { } duration ? $"{DisplayFormat.Weight(best.WeightKg)} · {DisplayFormat.Clock(duration)}" : DisplayFormat.Weight(best.WeightKg);

    private static string SetText(StrengthSet set)
    {
        var parts = new[] { set.WeightKg > 0m ? DisplayFormat.Weight(set.WeightKg) : null, set.Duration is { } d ? DisplayFormat.Clock(d) : null }.OfType<string>().ToList();
        return parts.Count > 0 ? string.Join(" ", parts) : "—";
    }
}
