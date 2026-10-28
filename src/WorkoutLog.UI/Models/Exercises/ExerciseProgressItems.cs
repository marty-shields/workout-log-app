using System.Globalization;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Components.Charts;
using WorkoutLog.UI.Components.Inputs;
using WorkoutLog.UI.Components.Lists;

namespace WorkoutLog.UI.Models.Exercises;

/// <summary>
/// Builds the text of a lift's Exercise page from its <see cref="LiftProgress"/> (FR-61), and the text every Exercise page shares. Dates are in the phone's time zone,
/// with the year only when it isn't this year (D49, D191).
/// </summary>
public static class ExerciseProgressItems
{
    /// <summary>Key of the Top set tab.</summary>
    public const string TopSetTab = "top-set";

    /// <summary>Key of the e1RM tab.</summary>
    public const string E1rmTab = "e1rm";

    /// <summary>The Top set tab.</summary>
    public static RadioOption<string> TopSetOption { get; } = new(TopSetTab, "Top set", $"exercise-tab-{TopSetTab}");

    /// <summary>The e1RM tab.</summary>
    public static RadioOption<string> E1rmOption { get; } = new(E1rmTab, "e1RM", $"exercise-tab-{E1rmTab}");

    /// <summary>The lift tabs: Top set, e1RM.</summary>
    public static IReadOnlyList<RadioOption<string>> Tabs { get; } = [TopSetOption, E1rmOption];

    /// <summary>Weight on the Top set chart, e.g. 70kg.</summary>
    public static readonly Func<decimal, string> Weight = value => DisplayFormat.Weight(value);

    /// <summary>An estimated one-rep max to up to 1 decimal place, halves away from zero, e.g. 82.3kg or 80kg.</summary>
    public static readonly Func<decimal, string> E1rm = value =>
        $"{Math.Round(value, 1, MidpointRounding.AwayFromZero).ToString("0.#", CultureInfo.InvariantCulture)}kg";

    /// <summary>Primary muscle · equipment, e.g. Quadriceps · Barbell; the muscle only when the equipment is None.</summary>
    public static string Subtitle(ExerciseDefinition exercise)
    {
        var muscle = exercise.PrimaryMuscle is { } m ? MuscleText(m) : null;
        var equipment = exercise.Equipment is { } e and not Equipment.None ? EquipmentText(e) : null;
        return string.Join(" · ", new[] { muscle, equipment }.OfType<string>());
    }

    /// <summary>Caption under the range: "Estimated one-rep max" on the e1RM tab; none on Top set.</summary>
    public static string? Caption(string tab) => tab == E1rmTab ? "Estimated one-rep max" : null;

    /// <summary>
    /// One point per session with a value for <paramref name="tab"/>, oldest first: the top set's weight labelled ×reps, or the unrounded e1RM.
    /// </summary>
    public static IReadOnlyList<ChartPoint> Points(LiftProgress progress, string tab, TimeZoneInfo zone, DateTimeOffset now) =>
        progress.Sessions
            .Select(s => tab == E1rmTab
                ? s.E1rmKg is { } e1rm ? new ChartPoint(Day(s.Session.StartedAt, zone, now), e1rm) : null
                : s.TopSet is { WeightKg: { } kg } top ? new ChartPoint(Day(s.Session.StartedAt, zone, now), kg, top.Reps is { } reps ? $"×{reps}" : null) : null)
            .OfType<ChartPoint>()
            .ToList();

    /// <summary>
    /// Text in place of the chart when sessions are in the range but none has a value for <paramref name="tab"/> (<see cref="NoSets"/>).
    /// Null when the chart has points or there are no sessions, which shows the view's own message.
    /// </summary>
    public static string? NoDataMessage(LiftProgress progress, string tab, ChartRange range)
    {
        if (progress.Sessions.Count == 0 || progress.Sessions.Any(s => tab == E1rmTab ? s.E1rmKg is not null : s.TopSet is not null))
        {
            return null;
        }

        return NoSets(range);
    }

    /// <summary>Text in place of a chart with no points: No sets in the last 3 months, or No sets yet on All.</summary>
    public static string NoSets(ChartRange range) => range == ChartRange.All ? "No sets yet" : $"No sets in the {ChartRanges.Text(range)}";

    /// <summary>Heaviest weight (e.g. 70kg × 3) and Best e1RM (e.g. 82.3kg), each with its date; no value shows —.</summary>
    public static IReadOnlyList<RecordRow> Records(LiftProgress progress, TimeZoneInfo zone, DateTimeOffset now) =>
    [
        Row("Heaviest weight", progress.Records.HeaviestWeight, TopSet, zone, now),
        Row("Best e1RM", progress.Records.BestE1rmKg, E1rm, zone, now),
    ];

    /// <summary>A record row: its value as <paramref name="text"/> with its date, or no value and no date when there is no record.</summary>
    internal static RecordRow Row<T>(string label, ProgressRecord<T>? record, Func<T, string> text, TimeZoneInfo zone, DateTimeOffset now) =>
        record is null ? new RecordRow(label, null, null) : new RecordRow(label, text(record.Value), Day(record.StartedAt, zone, now));

    /// <summary>
    /// Each session newest first: its date, its top set (e.g. 70kg × 3) or, with no weighted set, its most reps (e.g. 8 reps), and every set
    /// in order (e.g. 60×8 · 8 reps).
    /// </summary>
    public static IReadOnlyList<ExerciseSession> Sessions(LiftProgress progress, TimeZoneInfo zone, DateTimeOffset now) =>
        progress.Sessions
            .OrderByDescending(s => s.Session.StartedAt)
            .Select(s => new ExerciseSession(
                s.Session.WorkoutId,
                DisplayFormat.ShortDate(Local(s.Session.StartedAt, zone), Local(now, zone)),
                Headline(s),
                string.Join(" · ", s.Session.Sets.Select(SetText))))
            .ToList();

    private static string Headline(LiftSessionProgress session) =>
        session.TopSet is { } top ? TopSet(top)
        : session.Session.Sets.Max(s => s.Reps) is { } reps ? $"{reps} reps"
        : "—";

    /// <summary>A top set, e.g. 70kg × 3.</summary>
    internal static string TopSet(ExerciseBest top) =>
        top.Reps is { } reps ? $"{DisplayFormat.Weight(top.WeightKg)} × {reps}" : DisplayFormat.Weight(top.WeightKg);

    private static string SetText(StrengthSet set) => set switch
    {
        { WeightKg: { } kg, Reps: { } reps } => $"{Number(kg)}×{reps}",
        { WeightKg: { } kg } => DisplayFormat.Weight(kg),
        { Reps: { } reps } => $"{reps} reps",
        _ => "—",
    };

    /// <summary>A weight without its unit, e.g. 62.5.</summary>
    internal static string Number(decimal value) => Decimals.Round2(value).ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>A day in the phone's time zone, e.g. 13 Oct.</summary>
    internal static string Day(DateTimeOffset value, TimeZoneInfo zone, DateTimeOffset now) =>
        DisplayFormat.DayMonth(Local(value, zone), Local(now, zone));

    /// <summary><paramref name="value"/> in <paramref name="zone"/>.</summary>
    internal static DateTime Local(DateTimeOffset value, TimeZoneInfo zone) => TimeZoneInfo.ConvertTime(value, zone).DateTime;

    private static string MuscleText(Muscle muscle) => muscle switch
    {
        Muscle.LowerBack => "Lower Back",
        Muscle.MiddleBack => "Middle Back",
        _ => muscle.ToString(),
    };

    private static string EquipmentText(Equipment equipment) => equipment switch
    {
        Equipment.EzBar => "EZ-Bar",
        Equipment.SmithMachine => "Smith Machine",
        Equipment.TrapBar => "Trap Bar",
        _ => equipment.ToString(),
    };
}
