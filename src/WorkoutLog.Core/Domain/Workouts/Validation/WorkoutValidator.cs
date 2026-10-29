using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Domain.Workouts.Validation;

/// <summary>Checks workouts against the definitions in <see cref="IExerciseCatalog"/>.</summary>
internal sealed class WorkoutValidator(IExerciseCatalog catalog) : IWorkoutValidator
{
    private readonly Dictionary<int, ExerciseDefinition> definitions = catalog.All.ToDictionary(e => e.Id);

    /// <inheritdoc />
    public WorkoutErrors Validate(IReadOnlyList<WorkoutExercise> exercises) =>
        new(Errors(exercises), exercises.Select(Validate).ToList());

    /// <inheritdoc />
    public ExerciseErrors Validate(WorkoutExercise exercise)
    {
        if (!definitions.TryGetValue(exercise.ExerciseId, out var definition))
        {
            return new ExerciseErrors(ExerciseError.UnknownExercise, []);
        }

        return exercise switch
        {
            StrengthExercise strength when definition.Type == ExerciseType.Strength =>
                new ExerciseErrors(SetCountError(strength.Sets.Count),
                    strength.Sets.Select(s => Check(definition, Values(s))).ToList()),
            CardioExercise cardio when definition.Type == ExerciseType.Cardio =>
                new ExerciseErrors(null, [Check(definition, Values(cardio.Entry))]),
            _ => new ExerciseErrors(ExerciseError.WrongType, []),
        };
    }

    private static List<WorkoutError> Errors(IReadOnlyList<WorkoutExercise> exercises)
    {
        var errors = new List<WorkoutError>();
        if (exercises.Count == 0)
        {
            errors.Add(WorkoutError.NoExercises);
        }

        if (exercises.Count > WorkoutLimits.MaxExercises)
        {
            errors.Add(WorkoutError.TooManyExercises);
        }

        if (exercises.DistinctBy(e => e.ExerciseId).Count() < exercises.Count)
        {
            errors.Add(WorkoutError.RepeatedExercise);
        }

        return errors;
    }

    private static ExerciseError? SetCountError(int count) => count switch
    {
        0 => ExerciseError.NoSets,
        > WorkoutLimits.MaxSets => ExerciseError.TooManySets,
        _ => null,
    };

    private static EntryErrors Check(ExerciseDefinition definition, IEnumerable<(Metric Metric, decimal? Value)> values)
    {
        var errors = new Dictionary<Metric, FieldError>();
        foreach (var (metric, value) in values)
        {
            if (Error(definition, metric, value) is { } error)
            {
                errors[metric] = error;
            }
        }

        return new EntryErrors(errors);
    }

    private static FieldError? Error(ExerciseDefinition definition, Metric metric, decimal? value) =>
        (definition.Metrics.FirstOrDefault(m => m.Metric == metric), value) switch
        {
            (null, null) => null,
            (null, _) => FieldError.NotShown,
            ({ Required: true }, null) => FieldError.Missing,
            (_, null) => null,
            (_, <= 0m) when MustBePositive(metric) => FieldError.NotPositive,
            (_, < 0m) => FieldError.Negative,
            ({ Metric: Metric.Duration }, { } seconds) when seconds != decimal.Truncate(seconds) => FieldError.NotWholeSeconds,
            (_, { } number) when number != Math.Round(number, MetricLimits.MaxDecimalPlaces) => FieldError.TooManyDecimals,
            (_, { } number) when number > MetricLimits.Max(definition, metric) => FieldError.TooLarge,
            _ => null,
        };

    private static bool MustBePositive(Metric metric) => metric is not (Metric.WeightKg or Metric.Incline);

    private static (Metric, decimal?)[] Values(StrengthSet set) =>
    [
        (Metric.Reps, set.Reps),
        (Metric.WeightKg, set.WeightKg),
        (Metric.Duration, Seconds(set.Duration)),
        (Metric.Distance, set.Distance),
    ];

    private static (Metric, decimal?)[] Values(CardioEntry entry) =>
    [
        (Metric.Duration, Seconds(entry.Duration)),
        (Metric.Distance, entry.Distance),
        (Metric.Floors, entry.Floors),
        (Metric.Level, entry.Level),
        (Metric.Incline, entry.Incline),
        (Metric.Calories, entry.Calories),
        (Metric.Rate, entry.Rate),
    ];

    private static decimal? Seconds(TimeSpan? duration) => duration is { } d ? (decimal)d.TotalSeconds : null;
}
