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

    /// <summary>Logged exercises, most recent first.</summary>
    public static IReadOnlyList<ExerciseListItem> All { get; } =
    [
        new(6, "Barbell Back Squat", ExerciseKind.Strength, "Tue 20 Oct · 70kg × 3"),
        new(218, "Machine Assisted Pull-up", ExerciseKind.Strength, "Tue 20 Oct · 0kg × 5"),
        new(258, "Plank", ExerciseKind.Strength, "Thu 15 Oct · 1:45"),
        new(264, "Push-up", ExerciseKind.Strength, "Tue 13 Oct · 25 reps"),
        new(311, "Outdoor Run", ExerciseKind.Cardio, "Sun 11 Oct · 5.2km"),
        new(187, "Farmer's Walk", ExerciseKind.Strength, "Thu 8 Oct · 32kg · 40m"),
        new(132, "Dumbbell Curl", ExerciseKind.Strength, "Mon 5 Oct · 14kg × 10"),
        new(315, "Jump Rope", ExerciseKind.Cardio, "Sun 12 Oct 2025 · 10:00"),
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
        new(8, "Tue 20 Oct", "70kg × 3", "60×8 · 62.5×8 · 70×3"),
        new(7, "Tue 13 Oct", "65kg × 8", "65×8 · 65×8 · 65×7"),
        new(6, "Tue 6 Oct", "65kg × 6", "62.5×8 · 65×6 · 65×6"),
        new(5, "Tue 29 Sep", "65kg × 5", "62.5×8 · 65×5 · 65×5"),
        new(4, "Tue 8 Sep", "62.5kg × 8", "60×8 · 62.5×8 · 62.5×8"),
        new(3, "Tue 1 Sep", "62.5kg × 6", "60×8 · 62.5×6 · 62.5×6"),
        new(2, "Tue 25 Aug", "60kg × 8", "57.5×8 · 60×8 · 60×8"),
        new(1, "Tue 18 Aug", "60kg × 6", "57.5×8 · 60×6 · 60×6"),
    ];

    private static readonly IReadOnlyList<RecordRow> BodyweightRecords =
        [new("Most reps in a set", "25 reps", "13 Oct"), new("Most reps in a session", "63 reps", "13 Oct"), new("Heaviest weight", null, null)];

    private static readonly IReadOnlyList<ExerciseSession> BodyweightSessions =
    [
        new(16, "Tue 13 Oct", "25 reps", "25 · 20 · 18"),
        new(15, "Tue 6 Oct", "22 reps", "22 · 19 · 17"),
        new(14, "Tue 22 Sep", "20 reps", "20 · 18 · 17"),
        new(13, "Tue 15 Sep", "18 reps", "18 · 18 · 16"),
        new(12, "Tue 8 Sep", "18 reps", "18 · 17 · 15"),
        new(11, "Tue 1 Sep", "15 reps", "15 · 15 · 15"),
    ];

    private static readonly IReadOnlyList<RadioOption<string>> HoldTabs = [Tab("duration", "Duration"), Tab("weight", "Weight")];

    private static readonly IReadOnlyList<RecordRow> HoldRecords = [new("Longest hold", "1:45", "15 Oct"), new("Heaviest weight", "10kg · 1:00", "15 Oct")];

    private static readonly IReadOnlyList<ExerciseSession> HoldSessions =
    [
        new(46, "Thu 15 Oct", "1:45", "1:45 · 10kg 1:00 · 1:20"),
        new(45, "Tue 13 Oct", "1:30", "1:30 · 1:15"),
        new(44, "Tue 6 Oct", "1:30", "1:30 · 7.5kg 0:50"),
        new(43, "Tue 29 Sep", "1:15", "1:15 · 5kg 1:00"),
        new(42, "Tue 22 Sep", "1:10", "1:10 · 5kg 0:45"),
        new(41, "Tue 15 Sep", "1:00", "1:00 · 0:50"),
    ];

    private static readonly IReadOnlyList<RadioOption<string>> CarryTabs = [Tab("weight", "Weight"), Tab("distance", "Distance")];

    private static readonly IReadOnlyList<RecordRow> CarryRecords = [new("Heaviest weight", "32kg · 40m", "8 Oct"), new("Longest distance", "60m · 24kg", "15 Sep")];

    private static readonly IReadOnlyList<ExerciseSession> CarrySessions =
    [
        new(55, "Thu 8 Oct", "32kg · 40m", "28kg 40m · 32kg 40m · 32kg 30m"),
        new(54, "Tue 6 Oct", "30kg · 40m", "28kg 40m · 30kg 40m"),
        new(53, "Tue 22 Sep", "28kg · 40m", "26kg 40m · 28kg 40m"),
        new(52, "Tue 15 Sep", "26kg · 40m", "24kg 60m · 26kg 40m"),
        new(51, "Tue 8 Sep", "24kg · 40m", "24kg 40m · 24kg 40m"),
    ];

    private static readonly IReadOnlyList<RadioOption<string>> CardioTabs =
        [Tab("distance", "Distance"), Tab("duration", "Duration"), Tab("pace", "Pace"), Tab("calories", "Calories")];

    private static readonly IReadOnlyList<RecordRow> CardioRecords =
    [
        new("Longest distance", "5.2km", "11 Oct"),
        new("Longest duration", "30:00", "11 Oct"),
        new("Fastest pace", "5:44/km", "27 Sep"),
        new("Most calories", "410 calories", "11 Oct"),
    ];

    private static readonly IReadOnlyList<ExerciseSession> CardioSessions =
    [
        new(65, "Sun 11 Oct", "5.2km", "30:00 · 5:46/km · 410 calories"),
        new(64, "Sun 4 Oct", "5km", "29:20 · 5:52/km · 395 calories"),
        new(63, "Sun 27 Sep", "4.8km", "27:30 · 5:44/km · 360 calories"),
        new(62, "Sun 20 Sep", "5km", "29:00 · 5:48/km · 380 calories"),
        new(61, "Sun 13 Sep", "4.5km", "27:00 · 6:00/km · 330 calories"),
        new(60, "Sun 6 Sep", "4.1km", "25:00 · 6:06/km · 300 calories"),
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
        [
            new(25, "Tue 13 Oct", "16kg × 8", "16×8 · 16×8 · 14×10"),
            new(24, "Tue 6 Oct", "14kg × 10", "12×10 · 14×10"),
            new(23, "Tue 22 Sep", "12kg × 12", "12×10 · 12×12"),
            new(22, "Tue 15 Sep", "12kg × 10", "10×10 · 12×10"),
            new(21, "Tue 1 Sep", "10kg × 10", "10×10 · 10×10"),
        ]);

    /// <summary>An assisted machine: one chart of the least assist, lower is better.</summary>
    public static SampleExercise Assisted { get; } = new(
        "Machine Assisted Pull-up", "Lats · Machine", [], null,
        [P("8 Sep", 30, "×8"), P("15 Sep", 25, "×8"), P("22 Sep", 25, "×10"), P("29 Sep", 15, "×8"), P("13 Oct", 5, "×6"), P("20 Oct", 0, "×5")],
        Kg,
        [new("Least assist", "0kg × 5", "20 Oct")],
        [
            new(36, "Tue 20 Oct", "0kg × 5", "5kg×8 · 0kg×5 · 0kg×4"),
            new(35, "Tue 13 Oct", "5kg × 6", "10kg×8 · 5kg×6 · 5kg×5"),
            new(34, "Tue 29 Sep", "15kg × 8", "20kg×8 · 15kg×8"),
            new(33, "Tue 22 Sep", "25kg × 10", "30kg×8 · 25kg×10"),
            new(32, "Tue 15 Sep", "25kg × 8", "30kg×8 · 25kg×8"),
            new(31, "Tue 8 Sep", "30kg × 8", "35kg×8 · 30kg×8"),
        ],
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
        Points = [new("15 Sep", null), P("22 Sep", 5, "0:45"), P("29 Sep", 5, "1:00"), P("6 Oct", 7.5m, "0:50"), new("13 Oct", null), P("15 Oct", 10, "1:00")],
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
        Points = [P("6 Sep", 366), P("13 Sep", 360), P("20 Sep", 348), P("27 Sep", 344), P("4 Oct", 352), P("11 Oct", 346)],
        Format = Clock,
        TimeScale = true,
        Flipped = true,
        Caption = "Up = faster",
    };

    /// <summary>A treadmill on its Speed tab: speed worked out from distance and duration.</summary>
    public static SampleExercise CardioSpeed { get; } = new(
        "Treadmill", "Cardio", [Tab("distance", "Distance"), Tab("duration", "Duration"), Tab("speed", "Speed"), Tab("calories", "Calories")], "speed",
        [P("6 Sep", 9.6m), P("13 Sep", 9.84m), P("20 Sep", 10), P("27 Sep", 10.2m), P("4 Oct", 10.08m), P("11 Oct", 10.4m)],
        Count,
        [
            new("Longest distance", "5.2km", "11 Oct"),
            new("Longest duration", "30:00", "27 Sep"),
            new("Fastest speed", "10.4 km/h", "11 Oct"),
            new("Most calories", "410 calories", "11 Oct"),
        ],
        [
            new(75, "Sun 11 Oct", "5.2km", "30:00 · 10.4 km/h · 410 calories · 1.5%"),
            new(74, "Sun 4 Oct", "4.2km", "25:00 · 10.08 km/h · 330 calories · 1.5%"),
            new(73, "Sun 27 Sep", "5.1km", "30:00 · 10.2 km/h · 400 calories · 1%"),
            new(72, "Sun 20 Sep", "4km", "24:00 · 10 km/h · 310 calories"),
            new(71, "Sun 13 Sep", "4.1km", "25:00 · 9.84 km/h · 320 calories"),
            new(70, "Sun 6 Sep", "3.2km", "20:00 · 9.6 km/h · 250 calories"),
        ]);

    /// <summary>A cardio exercise on its Calories tab, with no calories logged.</summary>
    public static SampleExercise CardioNoData { get; } = CardioDistance with
    {
        Tab = "calories",
        Points = [],
        Format = Count,
        Records = [.. CardioRecords.Take(3), new("Most calories", null, null)],
        Sessions =
        [
            new(65, "Sun 11 Oct", "5.2km", "30:00 · 5:46/km"),
            new(64, "Sun 4 Oct", "5km", "29:20 · 5:52/km"),
            new(63, "Sun 27 Sep", "4.8km", "27:30 · 5:44/km"),
            new(62, "Sun 20 Sep", "5km", "29:00 · 5:48/km"),
            new(61, "Sun 13 Sep", "4.5km", "27:00 · 6:00/km"),
            new(60, "Sun 6 Sep", "4.1km", "25:00 · 6:06/km"),
        ],
        NoDataMessage = "No calories logged in the last 3 months",
    };

    /// <summary>A cardio exercise with no distance, on its Floors tab.</summary>
    public static SampleExercise CardioFloors { get; } = new(
        "Stair Climber", "Cardio", [Tab("duration", "Duration"), Tab("floors", "Floors"), Tab("calories", "Calories")], "floors",
        [P("10 Sep", 80), P("17 Sep", 90), P("24 Sep", 95), P("1 Oct", 110), P("8 Oct", 100), P("15 Oct", 120)],
        Count,
        [new("Longest duration", "25:00", "15 Oct"), new("Most floors", "120 floors", "15 Oct"), new("Most calories", "300 calories", "15 Oct")],
        [
            new(91, "Thu 15 Oct", "25:00", "120 floors · 300 calories · Speed level 8 · 70 steps/min"),
            new(90, "Thu 8 Oct", "20:00", "100 floors · 240 calories · Speed level 8"),
            new(89, "Thu 1 Oct", "22:00", "110 floors · 270 calories · Speed level 8"),
            new(88, "Thu 24 Sep", "20:00", "95 floors · 240 calories · Speed level 7"),
            new(87, "Thu 17 Sep", "20:00", "90 floors · 230 calories · Speed level 7"),
            new(86, "Thu 10 Sep", "18:00", "80 floors · 200 calories · Speed level 6"),
        ]);

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
