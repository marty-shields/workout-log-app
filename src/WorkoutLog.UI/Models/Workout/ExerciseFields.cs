using System.Globalization;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Models.Fields;

namespace WorkoutLog.UI.Models.Workout;

/// <summary>Builds the UI's view of an exercise from its definition.</summary>
public static class ExerciseFields
{
    /// <summary>Fields of each set or entry, in the definition's metric order.</summary>
    public static IReadOnlyList<FieldSpec> For(ExerciseDefinition exercise) =>
        exercise.Metrics.Select(m => Field(exercise, m)).ToList();

    /// <summary>A newly added exercise: one empty set (strength) or its empty entry (cardio).</summary>
    public static ExerciseState New(ExerciseDefinition exercise)
    {
        var state = State(exercise);
        state.Entries.Add(new EntryState());
        return state;
    }

    /// <summary>
    /// A saved exercise, with each value as the user would type it: decimals with up to 2 places and no trailing zeros, 0 included,
    /// and duration as h:mm:ss. Null when the record doesn't match the definition's type.
    /// </summary>
    public static ExerciseState? Saved(ExerciseDefinition exercise, WorkoutExercise saved)
    {
        var kind = Kind(exercise.Type);
        IEnumerable<Dictionary<Metric, object?>>? entries = (kind, saved) switch
        {
            (ExerciseKind.Strength, StrengthExercise strength) => strength.Sets.Select(Values),
            (ExerciseKind.Cardio, CardioExercise cardio) => [Values(cardio.Entry)],
            _ => null,
        };

        if (entries is null)
        {
            return null;
        }

        var state = State(exercise);
        foreach (var values in entries)
        {
            var entry = new EntryState();
            foreach (var field in state.Fields)
            {
                entry.Values[field.Key] = Text(field.Kind, values.GetValueOrDefault(field.Metric));
            }

            state.Entries.Add(entry);
        }

        return state;
    }

    /// <summary>
    /// The entry's worked-out value as shown under its fields, e.g. Pace 5:46/km or Speed 10.4 km/h; Pace — or Speed — until distance and
    /// duration are both readable and more than 0. Null when the exercise has none.
    /// </summary>
    public static string? WorkedOutText(ExerciseState exercise, EntryState entry)
    {
        if (exercise.WorkedOut == WorkedOutKind.None)
        {
            return null;
        }

        var duration = Value(exercise, entry, Metric.Duration) is { } seconds ? TimeSpan.FromSeconds((double)seconds) : (TimeSpan?)null;
        var distance = Value(exercise, entry, Metric.Distance);
        return exercise.WorkedOut == WorkedOutKind.Pace
            ? $"Pace {(WorkedOut.Pace(duration, distance) is { } pace ? PaceText(pace) : "—")}"
            : $"Speed {(WorkedOut.Speed(duration, distance) is { } speed ? SpeedText(speed) : "—")}";
    }

    /// <summary>A pace rounded to the second, e.g. 5:46/km.</summary>
    public static string PaceText(TimeSpan pace) => $"{DisplayFormat.Clock(WorkedOut.RoundPace(pace))}/km";

    /// <summary>A speed rounded to 2 decimal places with no trailing zeros, e.g. 10.4 km/h.</summary>
    public static string SpeedText(decimal speed) => $"{WorkedOut.RoundSpeed(speed).ToString("0.##", CultureInfo.InvariantCulture)} km/h";

    /// <summary>Puts each field's <see cref="FieldSpec.BlankFill"/> in the field when it is blank, e.g. 0 in a bodyweight move's weight.</summary>
    public static void FillBlanks(ExerciseState exercise, EntryState entry)
    {
        foreach (var field in exercise.Fields.Where(f => f.BlankFill is not null && string.IsNullOrEmpty(entry.Values.GetValueOrDefault(f.Key))))
        {
            entry.Values[field.Key] = field.BlankFill;
        }
    }

    /// <summary>Strength or cardio.</summary>
    public static ExerciseKind Kind(ExerciseType type) => type switch
    {
        ExerciseType.Strength => ExerciseKind.Strength,
        ExerciseType.Cardio => ExerciseKind.Cardio,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };

    private static ExerciseState State(ExerciseDefinition exercise) =>
        new(exercise.Id, exercise.Name, Kind(exercise.Type), For(exercise), WorkedOut.HasPace(exercise) ? WorkedOutKind.Pace
            : WorkedOut.HasSpeed(exercise) ? WorkedOutKind.Speed
            : WorkedOutKind.None);

    /// <summary>The field's value read as <see cref="SetChecker"/> reads it, when more than 0; otherwise null.</summary>
    private static decimal? Value(ExerciseState exercise, EntryState entry, Metric metric) =>
        exercise.Fields.FirstOrDefault(f => f.Metric == metric) is { } field
        && FieldReader.Read(field, entry.Values.GetValueOrDefault(field.Key)) is ({ } value and > 0m, null)
            ? value
            : null;

    private static FieldSpec Field(ExerciseDefinition exercise, MetricDefinition metric)
    {
        var required = metric.Required;
        var lift = exercise.Category == ExerciseCategory.Lift;
        return metric.Metric switch
        {
            Metric.Reps => new("reps", Metric.Reps, metric.Label ?? "Reps", FieldKind.Whole, required),
            Metric.WeightKg => new("weight", Metric.WeightKg, metric.Label ?? "Weight", FieldKind.Decimal, required, "kg",
                BlankFill: lift ? null : "0", HideZero: !lift && !exercise.IsAssisted),
            Metric.Duration => new("duration", Metric.Duration, metric.Label ?? "Duration", FieldKind.Duration, required),
            Metric.Distance => new("distance", Metric.Distance, metric.Label ?? "Distance", FieldKind.Decimal, required, DistanceText(exercise)),
            Metric.Floors => new("floors", Metric.Floors, metric.Label ?? "Floors", FieldKind.Whole, required),
            Metric.Level => new("level", Metric.Level, metric.Label ?? "Level", FieldKind.Whole, required),
            Metric.Incline => new("incline", Metric.Incline, metric.Label ?? "Incline", FieldKind.Decimal, required, "%"),
            Metric.Calories => new("calories", Metric.Calories, metric.Label ?? "Calories", FieldKind.Whole, required),
            Metric.Rate => new("rate", Metric.Rate, metric.Label ?? "Rate", FieldKind.Decimal, required, RateText(exercise)),
            _ => throw new ArgumentOutOfRangeException(nameof(metric), metric.Metric, null),
        };
    }

    private static Dictionary<Metric, object?> Values(StrengthSet set) => new()
    {
        [Metric.Reps] = set.Reps,
        [Metric.WeightKg] = set.WeightKg,
        [Metric.Duration] = set.Duration,
        [Metric.Distance] = set.Distance,
    };

    private static Dictionary<Metric, object?> Values(CardioEntry entry) => new()
    {
        [Metric.Duration] = entry.Duration,
        [Metric.Distance] = entry.Distance,
        [Metric.Floors] = entry.Floors,
        [Metric.Level] = entry.Level,
        [Metric.Incline] = entry.Incline,
        [Metric.Calories] = entry.Calories,
        [Metric.Rate] = entry.Rate,
    };

    /// <summary>The value as the field's text; null when there is none.</summary>
    private static string? Text(FieldKind kind, object? value) => (kind, value) switch
    {
        (_, null) => null,
        (FieldKind.Whole, int whole) => whole.ToString(CultureInfo.InvariantCulture),
        (FieldKind.Decimal, decimal number) => Decimals.Round2(number).ToString("0.##", CultureInfo.InvariantCulture),
        (FieldKind.Duration, TimeSpan time) => $"{(int)time.TotalHours}:{time.Minutes:00}:{time.Seconds:00}",
        _ => throw new ArgumentOutOfRangeException(nameof(value), kind, "Value doesn't fit the field"),
    };

    /// <summary>Unit of the exercise's distance, e.g. km.</summary>
    internal static string DistanceText(ExerciseDefinition exercise) => exercise.DistanceUnit switch
    {
        DistanceUnit.Km => "km",
        DistanceUnit.Metres => "m",
        DistanceUnit.Feet => "ft",
        _ => throw new ArgumentOutOfRangeException(nameof(exercise), exercise.DistanceUnit, "Distance needs a distance unit"),
    };

    /// <summary>A distance in the exercise's unit, to up to 2 decimal places, e.g. 5.2km or 40m.</summary>
    internal static string Distance(ExerciseDefinition exercise, decimal distance) =>
        $"{Decimals.Round2(distance).ToString("0.##", CultureInfo.InvariantCulture)}{DistanceText(exercise)}";

    /// <summary>Unit of the exercise's rate, e.g. rpm.</summary>
    internal static string RateText(ExerciseDefinition exercise) => exercise.RateUnit switch
    {
        RateUnit.Rpm => "rpm",
        RateUnit.StrokesPerMin => "strokes/min",
        RateUnit.StridesPerMin => "strides/min",
        RateUnit.StepsPerMin => "steps/min",
        RateUnit.FeetPerMin => "feet/min",
        _ => throw new ArgumentOutOfRangeException(nameof(exercise), exercise.RateUnit, "Rate needs a rate unit"),
    };

}
