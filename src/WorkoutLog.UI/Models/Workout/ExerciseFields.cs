using WorkoutLog.Core.Domain.Exercises;
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
            Metric.Rate when exercise.RateUnit == RateUnit.MinPerKm => new("rate", Metric.Rate, metric.Label ?? "Pace", FieldKind.Pace, required, "min/km"),
            Metric.Rate => new("rate", Metric.Rate, metric.Label ?? "Rate", FieldKind.Decimal, required, RateText(exercise)),
            _ => throw new ArgumentOutOfRangeException(nameof(metric), metric.Metric, null),
        };
    }

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
}
