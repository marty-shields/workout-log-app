using System.Globalization;
using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.UI.Components.ThisWeek;
using WorkoutLog.UI.Components.Trends;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary>
/// Sample data for the Trends stories and the UI tests. Today is Wed 18 Nov 2026. Each state is the parameters of a <see cref="TrendsView"/>, keyed by
/// parameter name, so a story and its test fill the view the same way.
/// </summary>
public static class SampleTrends
{
    public static DateOnly Today { get; } = new(2026, 11, 18);

    public static TrendTotals Nothing { get; } = new(0, TimeSpan.Zero, 0, 0);

    /// <summary>This week so far (Mon 16 to Wed 18 Nov): 3 workouts, 2h 35m, 34 sets, 9,840 kg.</summary>
    public static TrendTotals WeekTotals { get; } = new(3, new TimeSpan(2, 35, 0), 34, 9840);

    /// <summary>Last week to Wednesday: 2 workouts, 2h, 33 sets, 9,283 kg.</summary>
    public static TrendTotals WeekEarlier { get; } = new(2, new TimeSpan(2, 0, 0), 33, 9283);

    /// <summary>November to the 18th: 11 workouts, 10h 55m, 172 sets, 41,200 kg.</summary>
    public static TrendTotals MonthTotals { get; } = new(11, new TimeSpan(10, 55, 0), 172, 41200);

    /// <summary>October to the 18th: 10 workouts, 10h 15m, 163 sets, 39,615 kg.</summary>
    public static TrendTotals MonthEarlier { get; } = new(10, new TimeSpan(10, 15, 0), 163, 39615);

    /// <summary>2026 to 18 Nov: 146 workouts, 148h, 2,310 sets, 548 t.</summary>
    public static TrendTotals YearTotals { get; } = new(146, new TimeSpan(148, 0, 0), 2310, 548000);

    /// <summary>2025 to 18 Nov: 131 workouts, 136h, 2,120 sets, 503 t.</summary>
    public static TrendTotals YearEarlier { get; } = new(131, new TimeSpan(136, 0, 0), 2120, 503000);

    /// <summary>The current week, part-way through: Mon 16 to Wed 18 Nov.</summary>
    public static IReadOnlyDictionary<string, object?> Week => State(PeriodKind.Week, "16 – 22 Nov", canGoPrevious: true, p =>
    {
        p[nameof(TrendsView.Totals)] = WeekTotals;
        p[nameof(TrendsView.Earlier)] = WeekEarlier;
        p[nameof(TrendsView.Against)] = "Against Monday to Wednesday last week";
        p[nameof(TrendsView.Days)] = Days([62, 35, 58, 0, 0, 0, 0], [65, 0, 55, 0, 60, 70, 0], today: 2);
        p[nameof(TrendsView.Muscles)] = SampleThisWeek.Muscles;
        p[nameof(TrendsView.Records)] = SampleThisWeek.Records;
        p[nameof(TrendsView.RecentWeeks)] = RecentWeeks;
        p[nameof(TrendsView.Streak)] = 23;
    });

    /// <summary>A finished week, 9 to 15 Nov.</summary>
    public static IReadOnlyDictionary<string, object?> PastWeek => State(PeriodKind.Week, "9 – 15 Nov", canGoPrevious: true, canGoNext: true, past: true, p =>
    {
        p[nameof(TrendsView.Totals)] = new TrendTotals(4, new TimeSpan(4, 10, 0), 67, 12100);
        p[nameof(TrendsView.Earlier)] = new TrendTotals(5, new TimeSpan(5, 0, 0), 70, 12800);
        p[nameof(TrendsView.Against)] = "Against the week before";
        p[nameof(TrendsView.Days)] = Days([65, 0, 55, 0, 60, 70, 0], [50, 40, 60, 0, 70, 80, 0], today: null);
        p[nameof(TrendsView.Muscles)] = SampleThisWeek.AllTrained;
        p[nameof(TrendsView.Records)] = SampleThisWeek.ManyRecords.Take(2).ToList();
        p[nameof(TrendsView.RecentWeeks)] = RecentWeeks;
        p[nameof(TrendsView.Streak)] = 23;
    });

    /// <summary>A week with no workouts.</summary>
    public static IReadOnlyDictionary<string, object?> EmptyWeek => State(PeriodKind.Week, "2 – 8 Nov", canGoPrevious: false, canGoNext: true, past: true, p =>
    {
        p[nameof(TrendsView.Totals)] = Nothing;
        p[nameof(TrendsView.Earlier)] = Nothing;
        p[nameof(TrendsView.Against)] = "Against the week before";
        p[nameof(TrendsView.Days)] = Days([0, 0, 0, 0, 0, 0, 0], [0, 0, 0, 0, 0, 0, 0], today: null);
        p[nameof(TrendsView.Muscles)] = SampleThisWeek.NoneTrained;
        p[nameof(TrendsView.RecentWeeks)] = new int[12];
    });

    /// <summary>The current month, part-way through: 1 to 18 Nov.</summary>
    public static IReadOnlyDictionary<string, object?> Month => State(PeriodKind.Month, "November 2026", canGoPrevious: true, p =>
    {
        p[nameof(TrendsView.Totals)] = MonthTotals;
        p[nameof(TrendsView.Earlier)] = MonthEarlier;
        p[nameof(TrendsView.Against)] = "Against October to the 18th";
        p[nameof(TrendsView.Start)] = new DateOnly(2026, 11, 1);
        p[nameof(TrendsView.Today)] = Today;
        p[nameof(TrendsView.Trained)] = DayNumbers(2, 4, 6, 7, 9, 11, 13, 14, 16, 17, 18).Select(d => new DateOnly(2026, 11, d)).ToList();
        p[nameof(TrendsView.Weeks)] = new MonthWeek[]
        {
            new("1 Nov", 0, TimeSpan.Zero),
            new("2 – 8 Nov", 4, new TimeSpan(4, 10, 0)),
            new("9 – 15 Nov", 4, new TimeSpan(4, 10, 0)),
            new("16 – 18 Nov", 3, new TimeSpan(2, 35, 0)),
        };
        p[nameof(TrendsView.Records)] = MonthRecords;
        p[nameof(TrendsView.MostTrained)] = MostTrained;
    });

    /// <summary>A finished month, October 2026.</summary>
    public static IReadOnlyDictionary<string, object?> PastMonth => State(PeriodKind.Month, "October 2026", canGoPrevious: true, canGoNext: true, past: true, p =>
    {
        p[nameof(TrendsView.Totals)] = new TrendTotals(15, new TimeSpan(14, 20, 0), 230, 52300);
        p[nameof(TrendsView.Earlier)] = new TrendTotals(14, new TimeSpan(13, 0, 0), 215, 49800);
        p[nameof(TrendsView.Against)] = "Against September";
        p[nameof(TrendsView.Start)] = new DateOnly(2026, 10, 1);
        p[nameof(TrendsView.Today)] = Today;
        p[nameof(TrendsView.Trained)] = DayNumbers(1, 3, 5, 7, 8, 10, 12, 14, 15, 17, 19, 22, 24, 27, 29).Select(d => new DateOnly(2026, 10, d)).ToList();
        p[nameof(TrendsView.Weeks)] = new MonthWeek[]
        {
            new("1 – 4 Oct", 2, new TimeSpan(2, 0, 0)),
            new("5 – 11 Oct", 4, new TimeSpan(4, 5, 0)),
            new("12 – 18 Oct", 4, new TimeSpan(3, 55, 0)),
            new("19 – 25 Oct", 3, new TimeSpan(2, 50, 0)),
            new("26 – 31 Oct", 2, new TimeSpan(1, 30, 0)),
        };
        p[nameof(TrendsView.Records)] = MonthRecords.Take(2).ToList();
        p[nameof(TrendsView.MostTrained)] = MostTrained;
    });

    /// <summary>A month with no workouts.</summary>
    public static IReadOnlyDictionary<string, object?> EmptyMonth => State(PeriodKind.Month, "August 2026", canGoPrevious: false, canGoNext: true, past: true, p =>
    {
        p[nameof(TrendsView.Totals)] = Nothing;
        p[nameof(TrendsView.Earlier)] = Nothing;
        p[nameof(TrendsView.Against)] = "Against July";
        p[nameof(TrendsView.Start)] = new DateOnly(2026, 8, 1);
        p[nameof(TrendsView.Today)] = Today;
        p[nameof(TrendsView.Weeks)] = new MonthWeek[]
        {
            new("1 – 2 Aug", 0, TimeSpan.Zero),
            new("3 – 9 Aug", 0, TimeSpan.Zero),
            new("10 – 16 Aug", 0, TimeSpan.Zero),
            new("17 – 23 Aug", 0, TimeSpan.Zero),
            new("24 – 30 Aug", 0, TimeSpan.Zero),
            new("31 Aug", 0, TimeSpan.Zero),
        };
    });

    /// <summary>The current year, part-way through: 1 Jan to 18 Nov 2026.</summary>
    public static IReadOnlyDictionary<string, object?> Year => State(PeriodKind.Year, "2026", canGoPrevious: true, p =>
    {
        p[nameof(TrendsView.Totals)] = YearTotals;
        p[nameof(TrendsView.Earlier)] = YearEarlier;
        p[nameof(TrendsView.Against)] = "Against 2025 to 18 Nov";
        p[nameof(TrendsView.Months)] = Months([14, 12, 16, 13, 17, 11, 15, 8, 14, 15, 11, 0], current: 10);
        p[nameof(TrendsView.PerWeek)] = 3.2;
        p[nameof(TrendsView.LongestStreak)] = 23;
        p[nameof(TrendsView.StreakRunning)] = true;
        p[nameof(TrendsView.RecordsByMonth)] = RecordMonths([5, 3, 4, 2, 6, 2, 3, 1, 3, 4, 4, 0]);
    });

    /// <summary>A finished year, 2025.</summary>
    public static IReadOnlyDictionary<string, object?> PastYear => State(PeriodKind.Year, "2025", canGoPrevious: true, canGoNext: true, past: true, p =>
    {
        p[nameof(TrendsView.Totals)] = new TrendTotals(141, new TimeSpan(141, 0, 0), 2200, 520000);
        p[nameof(TrendsView.Earlier)] = new TrendTotals(120, new TimeSpan(130, 0, 0), 1900, 470000);
        p[nameof(TrendsView.Against)] = "Against 2024";
        p[nameof(TrendsView.Months)] = Months([12, 10, 14, 11, 15, 9, 13, 10, 12, 13, 12, 10], current: -1);
        p[nameof(TrendsView.PerWeek)] = 2.7;
        p[nameof(TrendsView.LongestStreak)] = 14;
        p[nameof(TrendsView.RecordsByMonth)] = RecordMonths([4, 3, 5, 2, 4, 1, 3, 2, 3, 2, 3, 1]);
    });

    /// <summary>A year with no workouts.</summary>
    public static IReadOnlyDictionary<string, object?> EmptyYear => State(PeriodKind.Year, "2024", canGoPrevious: false, canGoNext: true, past: true, p =>
    {
        p[nameof(TrendsView.Totals)] = Nothing;
        p[nameof(TrendsView.Earlier)] = Nothing;
        p[nameof(TrendsView.Against)] = "Against 2023";
        p[nameof(TrendsView.Months)] = Months(new int[12], current: -1);
        p[nameof(TrendsView.RecordsByMonth)] = RecordMonths(new int[12]);
    });

    /// <summary>The Week tab while its data loads.</summary>
    public static IReadOnlyDictionary<string, object?> Loading => State(PeriodKind.Week, "16 – 22 Nov", canGoPrevious: true, p => p[nameof(TrendsView.Loading)] = true);

    /// <summary>The Week tab after its data failed to load.</summary>
    public static IReadOnlyDictionary<string, object?> LoadFailed => State(PeriodKind.Week, "16 – 22 Nov", canGoPrevious: true, p => p[nameof(TrendsView.Failed)] = true);

    /// <summary>Workouts in each of the last 12 weeks, oldest first.</summary>
    public static IReadOnlyList<int> RecentWeeks { get; } = [3, 4, 2, 4, 3, 4, 4, 3, 5, 4, 4, 3];

    /// <summary>Four records in November, newest first.</summary>
    public static IReadOnlyList<WeekRecord> MonthRecords { get; } =
    [
        new(22, "Squat", "100kg × 5", DayOfWeek.Wednesday, "Wed 18 Nov"),
        new(8, "Bench press", "82.5kg × 5", DayOfWeek.Monday, "Mon 16 Nov"),
        new(115, "Deadlift", "150kg × 3", DayOfWeek.Saturday, "Sat 14 Nov"),
        new(41, "Treadmill run", "5.2 km", DayOfWeek.Friday, "Fri 6 Nov"),
    ];

    /// <summary>The five exercises done in most sessions in November.</summary>
    public static IReadOnlyList<MeterRow> MostTrained { get; } =
        [new("Bench press", 5), new("Squat", 4), new("Deadlift", 3), new("Treadmill run", 3), new("Overhead press", 3)];

    /// <summary>The current week's days, Monday to Sunday.</summary>
    public static IReadOnlyList<TrendBar> WeekDays => (IReadOnlyList<TrendBar>)Week[nameof(TrendsView.Days)]!;

    /// <summary>The current year's months.</summary>
    public static IReadOnlyList<TrendBar> YearMonths => (IReadOnlyList<TrendBar>)Year[nameof(TrendsView.Months)]!;

    /// <summary>2025's months: no current month, so only the highest is labelled.</summary>
    public static IReadOnlyList<TrendBar> PastYearMonths => (IReadOnlyList<TrendBar>)PastYear[nameof(TrendsView.Months)]!;

    /// <summary>A year's months with no workouts.</summary>
    public static IReadOnlyList<TrendBar> EmptyYearMonths => (IReadOnlyList<TrendBar>)EmptyYear[nameof(TrendsView.Months)]!;

    /// <summary>Days trained in November up to the 18th.</summary>
    public static IReadOnlyCollection<DateOnly> MonthTrainedDays => (IReadOnlyCollection<DateOnly>)Month[nameof(TrendsView.Trained)]!;

    /// <summary>Days trained in October.</summary>
    public static IReadOnlyCollection<DateOnly> PastMonthTrainedDays => (IReadOnlyCollection<DateOnly>)PastMonth[nameof(TrendsView.Trained)]!;

    /// <summary>November's weeks up to the 18th.</summary>
    public static IReadOnlyList<MonthWeek> MonthWeeks => (IReadOnlyList<MonthWeek>)Month[nameof(TrendsView.Weeks)]!;

    /// <summary>October's five weeks.</summary>
    public static IReadOnlyList<MonthWeek> PastMonthWeeks => (IReadOnlyList<MonthWeek>)PastMonth[nameof(TrendsView.Weeks)]!;

    /// <summary>Records set in each month of 2026 up to November.</summary>
    public static IReadOnlyList<MeterRow> YearRecordMonths => (IReadOnlyList<MeterRow>)Year[nameof(TrendsView.RecordsByMonth)]!;

    private static readonly string[] DayNames = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];

    private static readonly string[] MonthNames =
        [.. Enumerable.Range(1, 12).Select(m => CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(m))];

    private static string Minutes(int minutes) => minutes >= 60 ? $"{minutes / 60}h {minutes % 60:00}m" : $"{minutes}m";

    private static string Workouts(int count) => count == 1 ? "1 workout" : $"{count} workouts";

    private static int[] DayNumbers(params int[] days) => days;

    private static Dictionary<string, object?> State(
        PeriodKind kind, string label, bool canGoPrevious, Action<Dictionary<string, object?>> fill) =>
        State(kind, label, canGoPrevious, canGoNext: false, past: false, fill);

    private static Dictionary<string, object?> State(
        PeriodKind kind, string label, bool canGoPrevious, bool canGoNext, bool past, Action<Dictionary<string, object?>> fill)
    {
        var parameters = new Dictionary<string, object?>
        {
            [nameof(TrendsView.Kind)] = kind,
            [nameof(TrendsView.Label)] = label,
            [nameof(TrendsView.CanGoPrevious)] = canGoPrevious,
            [nameof(TrendsView.CanGoNext)] = canGoNext,
            [nameof(TrendsView.Past)] = past,
        };
        fill(parameters);
        return parameters;
    }

    /// <summary>Minutes trained each day of a week beside the same day the week before; <paramref name="today"/> is the index of today, or null for a finished week.</summary>
    private static List<TrendBar> Days(int[] minutes, int[] earlier, int? today) =>
        [.. minutes.Select((value, i) => new TrendBar(
            DayNames[i][..3],
            DayNames[i],
            value,
            value > 0 ? Minutes(value) : today is { } t && i > t ? "still to come" : "rest day",
            earlier[i],
            earlier[i] > 0 ? Minutes(earlier[i]) : "rest day",
            Current: i == today))];

    /// <summary>Workouts in each month; <paramref name="current"/> is the index of this month, or -1. The current and highest months show their value.</summary>
    private static List<TrendBar> Months(int[] workouts, int current)
    {
        var highest = workouts.Max();
        return [.. workouts.Select((value, i) => new TrendBar(
            MonthNames[i][..1],
            MonthNames[i],
            value,
            value == 0 ? "no workouts" : Workouts(value),
            Current: i == current,
            Labelled: i == current || (value == highest && highest > 0)))];
    }

    private static List<MeterRow> RecordMonths(int[] records) =>
        [.. records.Select((value, i) => new MeterRow(MonthNames[i][..3], value)).Where((row, i) => i < 11 || row.Value > 0)];
}
