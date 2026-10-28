using System.Globalization;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Components.Charts;
using WorkoutLog.UI.Components.Inputs;
using WorkoutLog.UI.Components.Lists;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Models.Exercises;

/// <summary>
/// Builds the text of a cardio exercise's Exercise page from its <see cref="CardioProgress"/> (FR-64). Dates are in the phone's time zone,
/// with the year only when it isn't this year (D49, D191).
/// </summary>
public static class CardioProgressItems
{
    /// <summary>Key of the Distance tab.</summary>
    public const string DistanceTab = "distance";

    /// <summary>Key of the Duration tab.</summary>
    public const string DurationTab = "duration";

    /// <summary>Key of the Pace tab.</summary>
    public const string PaceTab = "pace";

    /// <summary>Key of the Floors tab.</summary>
    public const string FloorsTab = "floors";

    /// <summary>Key of the Calories tab.</summary>
    public const string CaloriesTab = "calories";

    private static readonly IReadOnlyList<(Metric Metric, string Key, string Label, string Record)> Charts =
    [
        (Metric.Distance, DistanceTab, "Distance", "Longest distance"),
        (Metric.Duration, DurationTab, "Duration", "Longest duration"),
        (Metric.Pace, PaceTab, "Pace", "Fastest pace"),
        (Metric.Floors, FloorsTab, "Floors", "Most floors"),
        (Metric.Calories, CaloriesTab, "Calories", "Most calories"),
    ];

    /// <summary>Line under the name: Cardio.</summary>
    public const string Subtitle = "Cardio";

    /// <summary>A tab per chart metric of the exercise, in the order Distance, Duration, Pace, Floors, Calories.</summary>
    public static IReadOnlyList<RadioOption<string>> Tabs(ExerciseDefinition exercise) =>
        Charted(exercise).Select(c => new RadioOption<string>(c.Key, c.Label, $"exercise-tab-{c.Key}")).ToList();

    /// <summary>Value text for <paramref name="tab"/>'s chart: distance in the exercise's unit (e.g. 5.2km), times as m:ss (h:mm:ss from an hour), counts as numbers.</summary>
    public static Func<decimal, string> Format(ExerciseDefinition exercise, string tab) => tab switch
    {
        DistanceTab => value => Distance(exercise, value),
        DurationTab or PaceTab => value => DisplayFormat.Clock(TimeSpan.FromSeconds((double)value)),
        _ => value => value.ToString("0.##", CultureInfo.InvariantCulture),
    };

    /// <summary>True for the tabs whose values are seconds: Duration and Pace.</summary>
    public static bool TimeScale(string tab) => tab is DurationTab or PaceTab;

    /// <summary>True for Pace, where lower is better.</summary>
    public static bool Flipped(string tab) => tab == PaceTab;

    /// <summary>Caption under the range: "Up = faster" on Pace; none on the others.</summary>
    public static string? Caption(string tab) => tab == PaceTab ? "Up = faster" : null;

    /// <summary>One point per session with a value for <paramref name="tab"/>, oldest first; times in seconds.</summary>
    public static IReadOnlyList<ChartPoint> Points(CardioProgress progress, string tab, TimeZoneInfo zone, DateTimeOffset now) =>
        progress.Sessions
            .Select(s => Value(s.Best, tab) is { } value ? new ChartPoint(Day(s.Session.StartedAt, zone, now), value) : null)
            .OfType<ChartPoint>()
            .ToList();

    /// <summary>
    /// Text in place of the chart when sessions are in the range but none has a value for <paramref name="tab"/>, e.g. No calories logged in the last 3 months.
    /// Null when the chart has points or there are no sessions, which shows the view's own message.
    /// </summary>
    public static string? NoDataMessage(CardioProgress progress, string tab, ChartRange range)
    {
        if (progress.Sessions.Count == 0 || progress.Sessions.Any(s => Value(s.Best, tab) is not null))
        {
            return null;
        }

        var what = $"No {tab} logged";
        return range == ChartRange.All ? $"{what} yet" : $"{what} in the {ChartRanges.Text(range)}";
    }

    /// <summary>
    /// A record per chart metric of the exercise, e.g. Longest distance 5.2km, Fastest pace 5:30/km, Most calories 410 calories, each with its date;
    /// no value shows —.
    /// </summary>
    public static IReadOnlyList<RecordRow> Records(ExerciseDefinition exercise, CardioProgress progress, TimeZoneInfo zone, DateTimeOffset now)
    {
        var records = progress.Records;
        return Charted(exercise)
            .Select(c => c.Metric switch
            {
                Metric.Distance => Row(c.Record, records.LongestDistance, v => Distance(exercise, v), zone, now),
                Metric.Duration => Row(c.Record, records.LongestDuration, DisplayFormat.Clock, zone, now),
                Metric.Pace => Row(c.Record, records.FastestPace, v => Pace(exercise, v), zone, now),
                Metric.Floors => Row(c.Record, records.MostFloors, Floors, zone, now),
                _ => Row(c.Record, records.MostCalories, Calories, zone, now),
            })
            .ToList();
    }

    /// <summary>
    /// Each session newest first: its date, its longest distance (or, with no distance metric, its longest duration) and its entry's other values,
    /// e.g. 30:00 · 5:46/km · 410 calories · Resistance 8 · 2% · 85rpm. A session with two or more entries shows every value of each, joined with " / ".
    /// </summary>
    public static IReadOnlyList<ExerciseSession> Sessions(ExerciseDefinition exercise, CardioProgress progress, TimeZoneInfo zone, DateTimeOffset now)
    {
        var headline = HasMetric(exercise, Metric.Distance) ? Metric.Distance : Metric.Duration;
        return progress.Sessions
            .OrderByDescending(s => s.Session.StartedAt)
            .Select(s => new ExerciseSession(
                s.Session.WorkoutId,
                DisplayFormat.ShortDate(Local(s.Session.StartedAt, zone), Local(now, zone)),
                Headline(exercise, s.Best, headline),
                s.Session.Entries.Count == 1
                    ? EntryText(exercise, s.Session.Entries[0], headline)
                    : string.Join(" / ", s.Session.Entries.Select(e => EntryText(exercise, e, null)))))
            .ToList();
    }

    private static IEnumerable<(Metric Metric, string Key, string Label, string Record)> Charted(ExerciseDefinition exercise) =>
        Charts.Where(c => HasMetric(exercise, c.Metric));

    private static bool HasMetric(ExerciseDefinition exercise, Metric metric) => exercise.Metrics.Any(m => m.Metric == metric);

    private static decimal? Value(CardioBest best, string tab) => tab switch
    {
        DistanceTab => best.Distance,
        DurationTab => Seconds(best.Duration),
        PaceTab => Seconds(best.Pace),
        FloorsTab => best.Floors,
        CaloriesTab => best.Calories,
        _ => null,
    };

    private static decimal? Seconds(TimeSpan? time) => time is { } value ? (decimal)value.TotalSeconds : null;

    private static string Headline(ExerciseDefinition exercise, CardioBest best, Metric headline) => headline switch
    {
        Metric.Distance => best.Distance is { } distance ? Distance(exercise, distance) : "—",
        _ => best.Duration is { } duration ? DisplayFormat.Clock(duration) : "—",
    };

    /// <summary>The entry's values in the order Distance, Duration, Pace, Floors, Calories, Level, Incline, Rate, leaving out <paramref name="skip"/>.</summary>
    private static string EntryText(ExerciseDefinition exercise, CardioEntry entry, Metric? skip)
    {
        var level = exercise.Metrics.FirstOrDefault(m => m.Metric == Metric.Level)?.Label ?? "Level";
        (Metric Metric, string? Text)[] values =
        [
            (Metric.Distance, entry.Distance is { } distance ? Distance(exercise, distance) : null),
            (Metric.Duration, entry.Duration is { } duration ? DisplayFormat.Clock(duration) : null),
            (Metric.Pace, entry.Pace is { } pace ? Pace(exercise, pace) : null),
            (Metric.Floors, entry.Floors is { } floors ? Floors(floors) : null),
            (Metric.Calories, entry.Calories is { } calories ? Calories(calories) : null),
            (Metric.Level, entry.Level is { } value ? $"{level} {value}" : null),
            (Metric.Incline, entry.Incline is { } incline ? $"{Number(incline)}%" : null),
            (Metric.Rate, entry.Rate is { } rate ? Rate(exercise, rate) : null),
        ];
        return string.Join(" · ", values.Where(v => v.Metric != skip && v.Text is not null).Select(v => v.Text));
    }

    private static RecordRow Row<T>(string label, ProgressRecord<T>? record, Func<T, string> text, TimeZoneInfo zone, DateTimeOffset now) =>
        record is null ? new RecordRow(label, null, null) : new RecordRow(label, text(record.Value), Day(record.StartedAt, zone, now));

    private static string Distance(ExerciseDefinition exercise, decimal distance) => $"{Number(distance)}{ExerciseFields.DistanceText(exercise)}";

    private static string Pace(ExerciseDefinition exercise, TimeSpan pace) => exercise.PaceUnit switch
    {
        PaceUnit.PerKm => $"{DisplayFormat.Clock(pace)}/km",
        _ => throw new ArgumentOutOfRangeException(nameof(exercise), exercise.PaceUnit, "Pace needs a pace unit"),
    };

    /// <summary>Rate with its unit: no space before a symbol (85rpm, 12km/h), a space before a word (28 strokes/min).</summary>
    private static string Rate(ExerciseDefinition exercise, decimal rate)
    {
        var unit = ExerciseFields.RateText(exercise);
        return unit.EndsWith("/min", StringComparison.Ordinal) ? $"{Number(rate)} {unit}" : $"{Number(rate)}{unit}";
    }

    private static string Floors(int floors) => $"{floors} floors";

    private static string Calories(int calories) => $"{calories} calories";

    private static string Number(decimal value) => Decimals.Round2(value).ToString("0.##", CultureInfo.InvariantCulture);

    private static string Day(DateTimeOffset value, TimeZoneInfo zone, DateTimeOffset now) =>
        DisplayFormat.DayMonth(Local(value, zone), Local(now, zone));

    private static DateTime Local(DateTimeOffset value, TimeZoneInfo zone) => TimeZoneInfo.ConvertTime(value, zone).DateTime;
}
