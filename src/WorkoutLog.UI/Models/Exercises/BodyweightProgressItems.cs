using System.Globalization;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Components.Charts;
using WorkoutLog.UI.Components.Inputs;
using WorkoutLog.UI.Components.Lists;

namespace WorkoutLog.UI.Models.Exercises;

/// <summary>
/// Builds the text of a bodyweight move's Exercise page from its <see cref="BodyweightProgress"/> (FR-62). A set with no weight or 0 kg is
/// unweighted. Dates are in the phone's time zone, with the year only when it isn't this year (D49, D191).
/// </summary>
public static class BodyweightProgressItems
{
    /// <summary>Key of the Best set tab.</summary>
    public const string BestSetTab = "best-set";

    /// <summary>Key of the Total tab.</summary>
    public const string TotalTab = "total";

    /// <summary>Reps on the Best set and Total charts, e.g. 25.</summary>
    public static readonly Func<decimal, string> Reps = value => value.ToString("0", CultureInfo.InvariantCulture);

    /// <summary>Best set, Total, Top set; then e1RM when the move is loaded (D193).</summary>
    public static IReadOnlyList<RadioOption<string>> Tabs(bool loaded)
    {
        List<RadioOption<string>> tabs =
        [
            new(BestSetTab, "Best set", $"exercise-tab-{BestSetTab}"),
            new(TotalTab, "Total", $"exercise-tab-{TotalTab}"),
            ExerciseProgressItems.TopSetOption,
        ];
        if (loaded)
        {
            tabs.Add(ExerciseProgressItems.E1rmOption);
        }

        return tabs;
    }

    /// <summary>The chart's value format for <paramref name="tab"/>: reps on Best set and Total, otherwise as on a lift.</summary>
    public static Func<decimal, string> Format(string tab) => tab switch
    {
        BestSetTab or TotalTab => Reps,
        ExerciseProgressItems.E1rmTab => ExerciseProgressItems.E1rm,
        _ => ExerciseProgressItems.Weight,
    };

    /// <summary>
    /// One point per session with a value for <paramref name="tab"/>, oldest first: the best set's reps, the total reps, the top set's weight
    /// labelled ×reps, or the unrounded e1RM.
    /// </summary>
    public static IReadOnlyList<ChartPoint> Points(BodyweightProgress progress, string tab, TimeZoneInfo zone, DateTimeOffset now) =>
        progress.Sessions
            .Select(s => (Day: ExerciseProgressItems.Day(s.Session.StartedAt, zone, now), Session: s))
            .Select(p => tab switch
            {
                BestSetTab => p.Session.BestSetReps is { } reps ? new ChartPoint(p.Day, reps) : null,
                TotalTab => p.Session.TotalReps is { } total ? new ChartPoint(p.Day, total) : null,
                ExerciseProgressItems.E1rmTab => p.Session.E1rmKg is { } e1rm ? new ChartPoint(p.Day, e1rm) : null,
                _ => p.Session.TopSet is { WeightKg: { } kg } top ? new ChartPoint(p.Day, kg, top.Reps is { } reps ? $"×{reps}" : null) : null,
            })
            .OfType<ChartPoint>()
            .ToList();

    /// <summary>
    /// <see cref="ExerciseProgressItems.NoSets"/> when sessions are in the range but none has a value for <paramref name="tab"/>.
    /// Null when the chart has points or there are no sessions, which shows the view's own message.
    /// </summary>
    public static string? NoDataMessage(BodyweightProgress progress, string tab, ChartRange range) =>
        progress.Sessions.Count == 0 || progress.Sessions.Any(s => tab switch
        {
            BestSetTab => s.BestSetReps is not null,
            TotalTab => s.TotalReps is not null,
            ExerciseProgressItems.E1rmTab => s.E1rmKg is not null,
            _ => s.TopSet is not null,
        })
            ? null
            : ExerciseProgressItems.NoSets(range);

    /// <summary>
    /// Most reps in a set (e.g. 25 reps), Most reps in a session (e.g. 63 reps), Heaviest weight (e.g. 16kg × 8) and, when the move is loaded,
    /// Best e1RM (e.g. 20.3kg), each with its date; no value shows —.
    /// </summary>
    public static IReadOnlyList<RecordRow> Records(BodyweightProgress progress, bool loaded, TimeZoneInfo zone, DateTimeOffset now)
    {
        var records = progress.Records;
        List<RecordRow> rows =
        [
            ExerciseProgressItems.Row("Most reps in a set", records.MostRepsInSet, r => $"{r} reps", zone, now),
            ExerciseProgressItems.Row("Most reps in a session", records.MostRepsInSession, r => $"{r} reps", zone, now),
            ExerciseProgressItems.Row("Heaviest weight", records.HeaviestWeight, ExerciseProgressItems.TopSet, zone, now),
        ];
        if (loaded)
        {
            rows.Add(ExerciseProgressItems.Row("Best e1RM", records.BestE1rmKg, ExerciseProgressItems.E1rm, zone, now));
        }

        return rows;
    }

    /// <summary>
    /// Each session newest first: its date, its top set (e.g. 16kg × 8) or, with no set over 0 kg, its most reps (e.g. 25 reps), and every set
    /// in order (e.g. 16×8 · 20).
    /// </summary>
    public static IReadOnlyList<ExerciseSession> Sessions(BodyweightProgress progress, TimeZoneInfo zone, DateTimeOffset now) =>
        progress.Sessions
            .OrderByDescending(s => s.Session.StartedAt)
            .Select(s => new ExerciseSession(
                s.Session.WorkoutId,
                DisplayFormat.ShortDate(ExerciseProgressItems.Local(s.Session.StartedAt, zone), ExerciseProgressItems.Local(now, zone)),
                s.TopSet is { } top ? ExerciseProgressItems.TopSet(top) : s.BestSetReps is { } reps ? $"{reps} reps" : "—",
                string.Join(" · ", s.Session.Sets.Select(SetText))))
            .ToList();

    private static string SetText(StrengthSet set) => set switch
    {
        { WeightKg: { } kg, Reps: { } reps } when kg > 0m => $"{ExerciseProgressItems.Number(kg)}×{reps}",
        { WeightKg: { } kg } when kg > 0m => DisplayFormat.Weight(kg),
        { Reps: { } reps } => reps.ToString(CultureInfo.InvariantCulture),
        _ => "—",
    };
}
