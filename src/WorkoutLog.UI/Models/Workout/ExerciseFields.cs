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
        var state = new ExerciseState(exercise.Id, exercise.Name, Kind(exercise.Type), For(exercise));
        state.Entries.Add(new EntryState());
        return state;
    }

    /// <summary>
    /// A saved exercise, with each value as the user would type it: decimals with up to 2 places and no trailing zeros,
    /// duration as h:mm:ss, pace as m:ss. Null when the record doesn't match the definition's type.
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

        var state = new ExerciseState(exercise.Id, exercise.Name, kind, For(exercise));
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

    /// <summary>The exercise as the picker lists it.</summary>
    public static PickerItem PickerItem(ExerciseDefinition exercise) => new(exercise.Id, exercise.Name, Kind(exercise.Type));

    /// <summary>Strength or cardio.</summary>
    public static ExerciseKind Kind(ExerciseType type) => type switch
    {
        ExerciseType.Strength => ExerciseKind.Strength,
        ExerciseType.Cardio => ExerciseKind.Cardio,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };

    private static FieldSpec Field(ExerciseDefinition exercise, MetricDefinition metric)
    {
        var required = metric.Required;
        return metric.Metric switch
        {
            Metric.Reps => new("reps", Metric.Reps, metric.Label ?? "Reps", FieldKind.Whole, required),
            Metric.WeightKg => new("weight", Metric.WeightKg, metric.Label ?? "Weight", FieldKind.Decimal, required, "kg"),
            Metric.Duration => new("duration", Metric.Duration, metric.Label ?? "Duration", FieldKind.Duration, required),
            Metric.Distance => new("distance", Metric.Distance, metric.Label ?? "Distance", FieldKind.Decimal, required, DistanceText(exercise)),
            Metric.Floors => new("floors", Metric.Floors, metric.Label ?? "Floors", FieldKind.Whole, required),
            Metric.Level => new("level", Metric.Level, metric.Label ?? "Level", FieldKind.Whole, required),
            Metric.Incline => new("incline", Metric.Incline, metric.Label ?? "Incline", FieldKind.Decimal, required, "%"),
            Metric.Calories => new("calories", Metric.Calories, metric.Label ?? "Calories", FieldKind.Whole, required),
            Metric.Rate => new("rate", Metric.Rate, metric.Label ?? "Rate", FieldKind.Decimal, required, RateText(exercise)),
            Metric.Pace => new("pace", Metric.Pace, metric.Label ?? "Pace", FieldKind.Pace, required, PaceText(exercise)),
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
        [Metric.Pace] = entry.Pace,
    };

    /// <summary>The value as the field's text; null when there is none.</summary>
    private static string? Text(FieldKind kind, object? value) => (kind, value) switch
    {
        (_, null) => null,
        (FieldKind.Whole, int whole) => whole.ToString(CultureInfo.InvariantCulture),
        (FieldKind.Decimal, decimal number) => Decimals.Round2(number).ToString("0.##", CultureInfo.InvariantCulture),
        (FieldKind.Duration, TimeSpan time) => $"{(int)time.TotalHours}:{time.Minutes:00}:{time.Seconds:00}",
        (FieldKind.Pace, TimeSpan time) => $"{(int)time.TotalMinutes}:{time.Seconds:00}",
        _ => throw new ArgumentOutOfRangeException(nameof(value), kind, "Value doesn't fit the field"),
    };

    private static string DistanceText(ExerciseDefinition exercise) => exercise.DistanceUnit switch
    {
        DistanceUnit.Km => "km",
        DistanceUnit.Metres => "m",
        DistanceUnit.Feet => "ft",
        _ => throw new ArgumentOutOfRangeException(nameof(exercise), exercise.DistanceUnit, "Distance needs a distance unit"),
    };

    private static string RateText(ExerciseDefinition exercise) => exercise.RateUnit switch
    {
        RateUnit.KmPerHour => "km/h",
        RateUnit.Rpm => "rpm",
        RateUnit.StrokesPerMin => "strokes/min",
        RateUnit.StridesPerMin => "strides/min",
        RateUnit.StepsPerMin => "steps/min",
        RateUnit.FeetPerMin => "feet/min",
        _ => throw new ArgumentOutOfRangeException(nameof(exercise), exercise.RateUnit, "Rate needs a rate unit"),
    };

    private static string PaceText(ExerciseDefinition exercise) => exercise.PaceUnit switch
    {
        PaceUnit.PerKm => "min/km",
        _ => throw new ArgumentOutOfRangeException(nameof(exercise), exercise.PaceUnit, "Pace needs a pace unit"),
    };
}
