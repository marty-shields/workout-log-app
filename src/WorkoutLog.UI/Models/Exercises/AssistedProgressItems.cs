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

    /// <summary>One point per session, oldest first: the least assist labelled ×reps. A session with no set has a point with no value.</summary>
    public static IReadOnlyList<ChartPoint> Points(AssistedProgress progress, TimeZoneInfo zone, DateTimeOffset now) =>
        progress.Sessions.Select(s => ExerciseProgressItems.Point(s.Session.StartedAt, s.LeastAssist, zone, now)).ToList();

    /// <summary>Least assist (e.g. 0kg × 5) with its date; no value shows —.</summary>
    public static IReadOnlyList<RecordRow> Records(AssistedProgress progress, TimeZoneInfo zone, DateTimeOffset now) =>
        [ExerciseProgressItems.Row("Least assist", progress.Records.LeastAssist, Assist, zone, now)];

    /// <summary>Each session newest first: its date, its least assist (e.g. 0kg × 5), and every set in order (e.g. 5kg×8 · 0kg×5).</summary>
    public static IReadOnlyList<ExerciseSession> Sessions(AssistedProgress progress, TimeZoneInfo zone, DateTimeOffset now) =>
        progress.Sessions
            .OrderByDescending(s => s.Session.StartedAt)
            .Select(s => ExerciseProgressItems.Session(
                s.Session, s.LeastAssist is { } least ? Assist(least) : "—", string.Join(" · ", s.Session.Sets.Select(SetText)), zone, now))
            .ToList();

    private static string Assist(ExerciseBest best) =>
        best.Reps is { } reps ? $"{DisplayFormat.Weight(best.WeightKg)} × {reps}" : DisplayFormat.Weight(best.WeightKg);

    private static string SetText(StrengthSet set) =>
        set.Reps is { } reps ? $"{DisplayFormat.Weight(set.WeightKg ?? 0m)}×{reps}" : DisplayFormat.Weight(set.WeightKg ?? 0m);
}
