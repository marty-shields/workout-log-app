using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Components.Charts;
using WorkoutLog.UI.Components.Lists;

namespace WorkoutLog.UI.Models.Exercises;

/// <summary>Builds the text of an assisted machine's Exercise page from its <see cref="AssistedProgress"/> (FR-63). A blank assist shows as 0kg.</summary>
public static class AssistedProgressItems
{
    /// <summary>Title of the one chart.</summary>
    public const string ChartTitle = "Least assist per session";

    /// <summary>Caption under the range.</summary>
    public const string Caption = "Up = less assist";

    /// <summary>One point per session with a set, oldest first: the least assist labelled ×reps.</summary>
    public static IReadOnlyList<ChartPoint> Points(AssistedProgress progress, TimeZoneInfo zone, DateTimeOffset now) =>
        progress.Sessions
            .Where(s => s.LeastAssist is not null)
            .Select(s => new ChartPoint(
                ExerciseProgressItems.Day(s.Session.StartedAt, zone, now),
                s.LeastAssist!.WeightKg,
                s.LeastAssist.Reps is { } reps ? $"×{reps}" : null))
            .ToList();

    /// <summary>No sets in the range text when sessions are in the range but none has a set; otherwise null, which shows the view's own message.</summary>
    public static string? NoDataMessage(AssistedProgress progress, ChartRange range) =>
        progress.Sessions.Count > 0 && progress.Sessions.All(s => s.LeastAssist is null) ? ExerciseProgressItems.NoSets(range) : null;

    /// <summary>Least assist (e.g. 0kg × 5) with its date; no value shows —.</summary>
    public static IReadOnlyList<RecordRow> Records(AssistedProgress progress, TimeZoneInfo zone, DateTimeOffset now) =>
    [
        progress.Records.LeastAssist is { } least
            ? new RecordRow("Least assist", Assist(least.Value), ExerciseProgressItems.Day(least.StartedAt, zone, now))
            : new RecordRow("Least assist", null, null),
    ];

    /// <summary>Each session newest first: its date, its least assist (e.g. 0kg × 5), and every set in order (e.g. 5kg×8 · 0kg×5).</summary>
    public static IReadOnlyList<ExerciseSession> Sessions(AssistedProgress progress, TimeZoneInfo zone, DateTimeOffset now) =>
        progress.Sessions
            .OrderByDescending(s => s.Session.StartedAt)
            .Select(s => new ExerciseSession(
                s.Session.WorkoutId,
                DisplayFormat.ShortDate(ExerciseProgressItems.Local(s.Session.StartedAt, zone), ExerciseProgressItems.Local(now, zone)),
                s.LeastAssist is { } least ? Assist(least) : "—",
                string.Join(" · ", s.Session.Sets.Select(SetText))))
            .ToList();

    private static string Assist(ExerciseBest best) =>
        best.Reps is { } reps ? $"{DisplayFormat.Weight(best.WeightKg)} × {reps}" : DisplayFormat.Weight(best.WeightKg);

    private static string SetText(StrengthSet set) =>
        set.Reps is { } reps ? $"{DisplayFormat.Weight(set.WeightKg ?? 0m)}×{reps}" : DisplayFormat.Weight(set.WeightKg ?? 0m);
}
