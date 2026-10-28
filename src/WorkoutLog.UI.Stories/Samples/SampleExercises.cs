using System.Globalization;
using WorkoutLog.UI.Components.Charts;
using WorkoutLog.UI.Components.Inputs;
using WorkoutLog.UI.Components.Lists;
using WorkoutLog.UI.Models.Exercises;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary>What an Exercise screen shows for one exercise and tab: the <c>ExerciseView</c> parameters other than range, state and callbacks.</summary>
public sealed record SampleExercise(
    string Name,
    string Subtitle,
    IReadOnlyList<RadioOption<string>> Tabs,
    string? Tab,
    IReadOnlyList<ChartPoint> Points,
    Func<decimal, string> Format,
    IReadOnlyList<RecordRow> Records,
    IReadOnlyList<ExerciseSession> Sessions,
    string? ChartTitle = null,
    bool Flipped = false,
    bool TimeScale = false,
    string? Caption = null,
    string? NoDataMessage = null);

/// <summary>Sample data for the Exercises and Exercise stories and their tests. Values follow D49.</summary>
public static class SampleExercises
{
    /// <summary>Weight, e.g. 82.3kg.</summary>
    public static readonly Func<decimal, string> Kg = value => $"{Number(value)}kg";

    /// <summary>A plain number, e.g. 25.</summary>
    public static readonly Func<decimal, string> Count = Number;

    /// <summary>Distance in metres, e.g. 40m.</summary>
    public static readonly Func<decimal, string> Metres = value => $"{Number(value)}m";

    /// <summary>Distance in km, e.g. 5.2km.</summary>
    public static readonly Func<decimal, string> Km = value => $"{Number(value)}km";

    /// <summary>Seconds as m:ss, e.g. 1:45.</summary>
    public static readonly Func<decimal, string> Clock = value => $"{(int)value / 60}:{(int)value % 60:00}";

    /// <summary>Logged exercises, most recent first; the strength exercises open their Exercise page.</summary>
    public static IReadOnlyList<ExerciseListItem> All { get; } =
    [
        new(6, "Barbell Back Squat", ExerciseKind.Strength, "Tue 20 Oct · 70kg × 3", Opens: true),
        new(264, "Push-up", ExerciseKind.Strength, "Tue 20 Oct · 25 reps", Opens: true),
        new(218, "Machine Assisted Pull-up", ExerciseKind.Strength, "Thu 15 Oct · 20kg × 10", Opens: true),
        new(258, "Plank", ExerciseKind.Strength, "Thu 15 Oct · 1:45", Opens: true),
        new(311, "Outdoor Run", ExerciseKind.Cardio, "Sun 11 Oct · 5.2km"),
        new(187, "Farmer's Walk", ExerciseKind.Strength, "Thu 8 Oct · 32kg · 40m", Opens: true),
        new(315, "Jump Rope", ExerciseKind.Cardio, "Mon 5 Oct · 10:00"),
        new(132, "Dumbbell Curl", ExerciseKind.Strength, "Wed 15 Oct 2025", Opens: true),
    ];

    /// <summary>The strength exercises in <see cref="All"/>.</summary>
    public static IReadOnlyList<ExerciseListItem> Strength { get; } = [.. All.Where(e => e.Kind == ExerciseKind.Strength)];

    /// <summary>The cardio exercises in <see cref="All"/>.</summary>
    public static IReadOnlyList<ExerciseListItem> Cardio { get; } = [.. All.Where(e => e.Kind == ExerciseKind.Cardio)];

    private static readonly IReadOnlyList<RadioOption<string>> LiftTabs = [Tab("top-set", "Top set"), Tab("e1rm", "e1RM")];

    private static readonly IReadOnlyList<RadioOption<string>> BodyweightTabs = [Tab("best-set", "Best set"), Tab("total", "Total"), Tab("top-set", "Top set")];

    private static readonly IReadOnlyList<RecordRow> LiftRecords = [new("Heaviest weight", "70kg × 3", "20 Oct"), new("Best e1RM", "82.3kg", "13 Oct")];

    private static readonly IReadOnlyList<ExerciseSession> LiftSessions =
    [
        new(3, "Tue 20 Oct", "70kg × 3", "60×8 · 62.5×8 · 70×3"),
        new(2, "Tue 13 Oct", "65kg × 8", "65×8 · 65×8 · 65×7"),
        new(1, "Tue 6 Oct", "65kg × 6", "62.5×8 · 65×6 · 65×6"),
    ];

    private static readonly IReadOnlyList<RecordRow> BodyweightRecords =
        [new("Most reps in a set", "25 reps", "13 Oct"), new("Most reps in a session", "63 reps", "13 Oct"), new("Heaviest weight", null, null)];

    private static readonly IReadOnlyList<ExerciseSession> BodyweightSessions =
    [
        new(12, "Tue 13 Oct", "25 reps", "25 · 20 · 18"),
        new(11, "Tue 6 Oct", "22 reps", "22 · 19 · 17"),
    ];

    private static readonly IReadOnlyList<RadioOption<string>> HoldTabs = [Tab("duration", "Duration"), Tab("weight", "Weight")];

    private static readonly IReadOnlyList<RecordRow> HoldRecords = [new("Longest hold", "1:45", "15 Oct"), new("Heaviest weight", "10kg · 1:00", "15 Oct")];

    private static readonly IReadOnlyList<ExerciseSession> HoldSessions = [new(41, "Thu 15 Oct", "1:45", "1:45 · 10kg 1:00 · 1:20")];

    private static readonly IReadOnlyList<RadioOption<string>> CarryTabs = [Tab("weight", "Weight"), Tab("distance", "Distance")];

    private static readonly IReadOnlyList<RecordRow> CarryRecords = [new("Heaviest weight", "32kg · 40m", "8 Oct"), new("Longest distance", "60m · 24kg", "15 Sep")];

    private static readonly IReadOnlyList<ExerciseSession> CarrySessions = [new(51, "Thu 8 Oct", "32kg · 40m", "28kg 40m · 32kg 40m · 32kg 30m")];

    private static readonly IReadOnlyList<RadioOption<string>> CardioTabs =
        [Tab("distance", "Distance"), Tab("duration", "Duration"), Tab("pace", "Pace"), Tab("calories", "Calories")];

    private static readonly IReadOnlyList<RecordRow> CardioRecords =
    [
        new("Longest distance", "5.2km", "11 Oct"),
        new("Longest duration", "30:00", "11 Oct"),
        new("Fastest pace", "5:30/km", "27 Sep"),
        new("Most calories", "410 calories", "11 Oct"),
    ];

    private static readonly IReadOnlyList<ExerciseSession> CardioSessions =
    [
        new(61, "Sun 11 Oct", "5.2km", "30:00 · 5:46/km · 410 calories"),
        new(60, "Sun 4 Oct", "5km", "29:20 · 5:52/km · 395 calories"),
    ];

    /// <summary>A barbell lift on its Top set tab.</summary>
    public static SampleExercise LiftTopSet { get; } = new(
        "Barbell Back Squat", "Quadriceps · Barbell", LiftTabs, "top-set",
        [P("18 Aug", 60, "×6"), P("25 Aug", 60, "×8"), P("1 Sep", 62.5m, "×6"), P("8 Sep", 62.5m, "×8"), P("29 Sep", 65, "×5"), P("6 Oct", 65, "×6"), P("13 Oct", 65, "×8"), P("20 Oct", 70, "×3")],
        Kg, LiftRecords, LiftSessions);

    /// <summary>A barbell lift on its e1RM tab.</summary>
    public static SampleExercise LiftE1rm { get; } = LiftTopSet with
    {
        Tab = "e1rm",
        Points = [P("18 Aug", 72.8m), P("25 Aug", 76), P("1 Sep", 76), P("8 Sep", 79.2m), P("29 Sep", 79.2m), P("6 Oct", 79.2m), P("13 Oct", 82.3m), P("20 Oct", 79.2m)],
        Caption = "Estimated one-rep max",
    };

    /// <summary>A barbell lift on its e1RM tab, with only sets over 12 reps.</summary>
    public static SampleExercise LiftE1rmNoData { get; } = LiftE1rm with
    {
        Points = [],
        Records = [new("Heaviest weight", "40kg × 15", "20 Oct"), new("Best e1RM", null, null)],
        Sessions = [new(81, "Tue 20 Oct", "40kg × 15", "40×15 · 40×15 · 40×14")],
        NoDataMessage = "No sets in the last 3 months",
    };

    /// <summary>A bodyweight move on its Best set tab.</summary>
    public static SampleExercise BodyweightBestSet { get; } = new(
        "Push-up", "Chest", BodyweightTabs, "best-set",
        [P("1 Sep", 15), P("8 Sep", 18), P("15 Sep", 18), P("22 Sep", 20), P("6 Oct", 22), P("13 Oct", 25)],
        Count, BodyweightRecords, BodyweightSessions);

    /// <summary>A bodyweight move on its Total tab.</summary>
    public static SampleExercise BodyweightTotal { get; } = BodyweightBestSet with
    {
        Tab = "total",
        Points = [P("1 Sep", 45), P("8 Sep", 50), P("15 Sep", 52), P("22 Sep", 55), P("6 Oct", 58), P("13 Oct", 63)],
    };

    /// <summary>A bodyweight move on its Top set tab, never done with weight.</summary>
    public static SampleExercise BodyweightTopSetNeverWeighted { get; } = BodyweightBestSet with
    {
        Tab = "top-set",
        Points = [],
        Format = Kg,
        NoDataMessage = "No sets in the last 3 months",
    };

    /// <summary>A bodyweight move done with dumbbells, on its Top set tab.</summary>
    public static SampleExercise LoadedBodyweight { get; } = new(
        "Bulgarian Split Squat", "Quadriceps", [.. BodyweightTabs, Tab("e1rm", "e1RM")], "top-set",
        [P("1 Sep", 10, "×10"), P("15 Sep", 12, "×10"), P("22 Sep", 12, "×12"), P("6 Oct", 14, "×10"), P("13 Oct", 16, "×8")],
        Kg,
        [new("Most reps in a set", null, null), new("Most reps in a session", null, null), new("Heaviest weight", "16kg × 8", "13 Oct"), new("Best e1RM", "20.3kg", "13 Oct")],
        [new(21, "Tue 13 Oct", "16kg × 8", "16×8 · 16×8 · 14×10")]);

    /// <summary>An assisted machine: one chart of the least assist, lower is better.</summary>
    public static SampleExercise Assisted { get; } = new(
        "Machine Assisted Pull-up", "Lats · Machine", [], null,
        [P("8 Sep", 30, "×8"), P("15 Sep", 25, "×8"), P("22 Sep", 25, "×10"), P("29 Sep", 15, "×8"), P("13 Oct", 5, "×6"), P("20 Oct", 0, "×5")],
        Kg,
        [new("Least assist", "0kg × 5", "20 Oct")],
        [new(31, "Tue 20 Oct", "0kg × 5", "5kg×8 · 0kg×5 · 0kg×4"), new(30, "Tue 13 Oct", "5kg × 6", "10kg×8 · 5kg×6 · 5kg×5")],
        ChartTitle: "Least assist per session",
        Flipped: true,
        Caption: "Up = less assist");

    /// <summary>A hold on its Duration tab.</summary>
    public static SampleExercise HoldDuration { get; } = new(
        "Plank", "Abdominals", HoldTabs, "duration",
        [P("15 Sep", 60), P("22 Sep", 70), P("29 Sep", 75), P("6 Oct", 90), P("13 Oct", 90), P("15 Oct", 105)],
        Clock, HoldRecords, HoldSessions, TimeScale: true);

    /// <summary>A hold on its Weight tab.</summary>
    public static SampleExercise HoldWeight { get; } = HoldDuration with
    {
        Tab = "weight",
        Points = [P("22 Sep", 5, "0:45"), P("29 Sep", 5, "1:00"), P("6 Oct", 7.5m, "0:50"), P("15 Oct", 10, "1:00")],
        Format = Kg,
        TimeScale = false,
    };

    /// <summary>A hold on its Weight tab, never done with weight.</summary>
    public static SampleExercise HoldWeightNoData { get; } = HoldWeight with
    {
        Points = [],
        Records = [new("Longest hold", "1:45", "15 Oct"), new("Heaviest weight", null, null)],
        Sessions = [new(41, "Thu 15 Oct", "1:45", "1:45 · 1:30 · 1:20")],
        NoDataMessage = "No sets in the last 3 months",
    };

    /// <summary>A carry on its Weight tab.</summary>
    public static SampleExercise CarryWeight { get; } = new(
        "Farmer's Walk", "Forearms · Other", CarryTabs, "weight",
        [P("8 Sep", 24, "40m"), P("15 Sep", 26, "40m"), P("22 Sep", 28, "40m"), P("6 Oct", 30, "40m"), P("8 Oct", 32, "40m")],
        Kg, CarryRecords, CarrySessions);

    /// <summary>A carry on its Distance tab.</summary>
    public static SampleExercise CarryDistance { get; } = CarryWeight with
    {
        Tab = "distance",
        Points = [P("8 Sep", 40, "24kg"), P("15 Sep", 60, "24kg"), P("22 Sep", 40, "28kg"), P("6 Oct", 40, "30kg"), P("8 Oct", 40, "32kg")],
        Format = Metres,
    };

    /// <summary>A carry on its Weight tab, never done with weight.</summary>
    public static SampleExercise CarryWeightNoData { get; } = CarryWeight with
    {
        Points = [],
        Records = [new("Heaviest weight", null, null), new("Longest distance", "60m", "8 Oct")],
        Sessions = [new(51, "Thu 8 Oct", "60m", "60m · 40m 0:30 · 40m")],
        NoDataMessage = "No sets in the last 3 months",
    };

    /// <summary>A cardio exercise on its Distance tab.</summary>
    public static SampleExercise CardioDistance { get; } = new(
        "Outdoor Run", "Cardio", CardioTabs, "distance",
        [P("6 Sep", 4.1m), P("13 Sep", 4.5m), P("20 Sep", 5), P("27 Sep", 4.8m), P("4 Oct", 5), P("11 Oct", 5.2m)],
        Km, CardioRecords, CardioSessions);

    /// <summary>A cardio exercise on its Duration tab.</summary>
    public static SampleExercise CardioDuration { get; } = CardioDistance with
    {
        Tab = "duration",
        Points = [P("6 Sep", 1500), P("13 Sep", 1620), P("20 Sep", 1740), P("27 Sep", 1650), P("4 Oct", 1760), P("11 Oct", 1800)],
        Format = Clock,
        TimeScale = true,
    };

    /// <summary>A cardio exercise on its Pace tab: lower is better.</summary>
    public static SampleExercise CardioPace { get; } = CardioDistance with
    {
        Tab = "pace",
        Points = [P("6 Sep", 340), P("13 Sep", 335), P("20 Sep", 338), P("27 Sep", 330), P("4 Oct", 352), P("11 Oct", 346)],
        Format = Clock,
        TimeScale = true,
        Flipped = true,
        Caption = "Up = faster",
    };

    /// <summary>A cardio exercise on its Calories tab, with no calories logged.</summary>
    public static SampleExercise CardioNoData { get; } = CardioDistance with
    {
        Tab = "calories",
        Points = [],
        Format = Count,
        Records = [.. CardioRecords.Take(3), new("Most calories", null, null)],
        NoDataMessage = "No calories logged in the last 3 months",
    };

    /// <summary>A barbell lift with no sessions in the range.</summary>
    public static SampleExercise NoSessionsInRange { get; } = new(
        "Barbell Bench Press", "Chest · Barbell", LiftTabs, "top-set", [], Kg,
        [new("Heaviest weight", null, null), new("Best e1RM", null, null)], []);

    /// <summary>A dumbbell lift with one session in the range.</summary>
    public static SampleExercise OneSession { get; } = new(
        "Dumbbell Curl", "Biceps · Dumbbell", LiftTabs, "top-set", [P("5 Oct", 14, "×10")], Kg,
        [new("Heaviest weight", "14kg × 10", "5 Oct"), new("Best e1RM", "18.7kg", "5 Oct")],
        [new(71, "Mon 5 Oct", "14kg × 10", "12×10 · 14×10 · 14×8")]);

    /// <summary>Twelve weekly top sets, for a chart with more points than it labels.</summary>
    public static IReadOnlyList<ChartPoint> ManyPoints { get; } =
    [
        P("4 Aug", 55, "×8"), P("11 Aug", 57.5m, "×8"), P("18 Aug", 60, "×6"), P("25 Aug", 60, "×8"),
        P("1 Sep", 62.5m, "×6"), P("8 Sep", 62.5m, "×8"), P("15 Sep", 60, "×8"), P("22 Sep", 65, "×5"),
        P("29 Sep", 65, "×6"), P("6 Oct", 70, "×3"), P("13 Oct", 65, "×8"), P("20 Oct", 67.5m, "×5"),
    ];

    private static RadioOption<string> Tab(string key, string label) => new(key, label, $"exercise-tab-{key}");

    private static ChartPoint P(string date, decimal value, string? label = null) => new(date, value, label);

    private static string Number(decimal value) => value.ToString("0.##", CultureInfo.InvariantCulture);
}
