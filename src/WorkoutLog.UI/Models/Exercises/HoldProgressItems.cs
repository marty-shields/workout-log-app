using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Components.Charts;
using WorkoutLog.UI.Components.Inputs;
using WorkoutLog.UI.Components.Lists;

namespace WorkoutLog.UI.Models.Exercises;

/// <summary>Builds the text of a hold's Exercise page from its <see cref="HoldProgress"/> (FR-63). A weight of 0kg or blank isn't shown.</summary>
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

    /// <summary>One point per session with a value for <paramref name="tab"/>, oldest first: the longest hold in seconds, or the heaviest weight labelled with its duration.</summary>
    public static IReadOnlyList<ChartPoint> Points(HoldProgress progress, string tab, TimeZoneInfo zone, DateTimeOffset now) =>
        progress.Sessions
            .Select(s => tab == WeightTab
                ? s.Heaviest is { WeightKg: { } kg } heaviest
                    ? new ChartPoint(ExerciseProgressItems.Day(s.Session.StartedAt, zone, now), kg, heaviest.Duration is { } d ? DisplayFormat.Clock(d) : null)
                    : null
                : s.Longest is { } longest ? new ChartPoint(ExerciseProgressItems.Day(s.Session.StartedAt, zone, now), (decimal)longest.TotalSeconds) : null)
            .OfType<ChartPoint>()
            .ToList();

    /// <summary>No sets in the range text when sessions are in the range but none has a value for <paramref name="tab"/>; otherwise null.</summary>
    public static string? NoDataMessage(HoldProgress progress, string tab, ChartRange range) =>
        progress.Sessions.Count > 0 && progress.Sessions.All(s => tab == WeightTab ? s.Heaviest is null : s.Longest is null)
            ? ExerciseProgressItems.NoSets(range)
            : null;

    /// <summary>Longest hold (e.g. 1:45) and Heaviest weight (e.g. 10kg · 1:00), each with its date; no value shows —.</summary>
    public static IReadOnlyList<RecordRow> Records(HoldProgress progress, TimeZoneInfo zone, DateTimeOffset now) =>
    [
        progress.Records.LongestHold is { } longest
            ? new RecordRow("Longest hold", DisplayFormat.Clock(longest.Value), ExerciseProgressItems.Day(longest.StartedAt, zone, now))
            : new RecordRow("Longest hold", null, null),
        progress.Records.HeaviestWeight is { } heaviest
            ? new RecordRow("Heaviest weight", Heaviest(heaviest.Value), ExerciseProgressItems.Day(heaviest.StartedAt, zone, now))
            : new RecordRow("Heaviest weight", null, null),
    ];

    /// <summary>Each session newest first: its date, its longest hold (e.g. 1:45), and every set in order (e.g. 1:45 · 10kg 1:00).</summary>
    public static IReadOnlyList<ExerciseSession> Sessions(HoldProgress progress, TimeZoneInfo zone, DateTimeOffset now) =>
        progress.Sessions
            .OrderByDescending(s => s.Session.StartedAt)
            .Select(s => new ExerciseSession(
                s.Session.WorkoutId,
                DisplayFormat.ShortDate(ExerciseProgressItems.Local(s.Session.StartedAt, zone), ExerciseProgressItems.Local(now, zone)),
                s.Longest is { } longest ? DisplayFormat.Clock(longest) : s.Heaviest is { } heaviest ? Heaviest(heaviest) : "—",
                string.Join(" · ", s.Session.Sets.Select(SetText))))
            .ToList();

    private static string Heaviest(ExerciseBest best) =>
        best.Duration is { } duration ? $"{DisplayFormat.Weight(best.WeightKg)} · {DisplayFormat.Clock(duration)}" : DisplayFormat.Weight(best.WeightKg);

    private static string SetText(StrengthSet set)
    {
        var parts = new[] { set.WeightKg > 0m ? DisplayFormat.Weight(set.WeightKg) : null, set.Duration is { } d ? DisplayFormat.Clock(d) : null }.OfType<string>().ToList();
        return parts.Count > 0 ? string.Join(" ", parts) : "—";
    }
}
