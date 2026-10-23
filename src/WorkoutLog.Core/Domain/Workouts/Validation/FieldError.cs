namespace WorkoutLog.Core.Domain.Workouts.Validation;

/// <summary>Why a metric of a set or entry can't be saved.</summary>
public enum FieldError
{
    /// <summary>Required by the exercise but empty.</summary>
    Missing,

    /// <summary>Must be more than 0: reps, required duration and required distance.</summary>
    NotPositive,

    /// <summary>Below 0.</summary>
    Negative,

    /// <summary>A duration or pace with part of a second.</summary>
    NotWholeSeconds,

    /// <summary>Above the metric's limit in <see cref="Exercises.MetricLimits"/>.</summary>
    TooLarge,

    /// <summary>Has a value, but the exercise doesn't show this metric.</summary>
    NotShown,
}
