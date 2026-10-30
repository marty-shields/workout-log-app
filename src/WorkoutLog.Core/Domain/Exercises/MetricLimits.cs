namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>Highest value each metric accepts.</summary>
public static class MetricLimits
{
    /// <summary>Highest duration: 23:59:59.</summary>
    public static readonly TimeSpan MaxDuration = new(23, 59, 59);

    /// <summary>Most decimal places a decimal metric accepts.</summary>
    public const int MaxDecimalPlaces = 2;

    /// <summary>Highest value of <paramref name="metric"/> for <paramref name="exercise"/>, in the exercise's unit. Duration is in seconds.</summary>
    public static decimal Max(ExerciseDefinition exercise, Metric metric) => metric switch
    {
        Metric.Reps => 1_000,
        Metric.WeightKg => 1_000,
        Metric.Duration => (decimal)MaxDuration.TotalSeconds,
        Metric.Distance => exercise.DistanceUnit switch
        {
            DistanceUnit.Km => 1_000,
            DistanceUnit.Metres or DistanceUnit.Feet => 100_000,
            _ => throw new ArgumentException($"{exercise.Name} has no distance unit", nameof(exercise)),
        },
        Metric.Floors => 1_000,
        Metric.Level => 100,
        Metric.Incline => 100,
        Metric.Calories => 10_000,
        Metric.Rate => exercise.RateUnit switch
        {
            RateUnit.Rpm or RateUnit.StridesPerMin or RateUnit.StepsPerMin => 300,
            RateUnit.StrokesPerMin => 200,
            RateUnit.FeetPerMin => 1_000,
            _ => throw new ArgumentException($"{exercise.Name} has no rate metric", nameof(exercise)),
        },
        _ => throw new ArgumentOutOfRangeException(nameof(metric), metric, "Unknown metric"),
    };
}
