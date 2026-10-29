using System.Globalization;
using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Logging;
using WorkoutLog.UI.Models.Fields;

namespace WorkoutLog.UI.Models.Workout;

/// <summary>
/// Reads each field's text, a blank one as its <see cref="FieldSpec.BlankFill"/>, then checks the set or entry with
/// <see cref="IWorkoutService.ValidateSet"/> or <see cref="IWorkoutService.ValidateEntry"/> and words its codes.
/// A field that can't be read gets a format message and no message from Core.
/// A Core code with no message, which the UI can't cause, is logged and fails the set or entry.
/// </summary>
public sealed partial class SetChecker(IWorkoutService workouts, IExerciseCatalog catalog, ILogger<SetChecker> logger) : ISetChecker
{
    private static readonly decimal MaxDurationSeconds = (decimal)MetricLimits.MaxDuration.TotalSeconds;

    /// <inheritdoc />
    public bool Check(ExerciseState exercise, EntryState entry) => Check(exercise, entry, logExerciseError: true);

    /// <inheritdoc />
    public bool CheckOnSave(ExerciseState exercise, EntryState entry)
    {
        ExerciseFields.FillBlanks(exercise, entry);
        return Check(exercise, entry, logExerciseError: false);
    }

    private bool Check(ExerciseState exercise, EntryState entry, bool logExerciseError)
    {
        entry.Errors.Clear();
        var values = Values(exercise, entry, entry.Errors);
        var result = exercise.Kind == ExerciseKind.Strength
            ? workouts.ValidateSet(exercise.ExerciseId, ToSet(values))
            : workouts.ValidateEntry(exercise.ExerciseId, ToEntry(values));
        var codes = result.Entries.Count > 0 ? result.Entries[0].Fields : new Dictionary<Metric, FieldError>();
        foreach (var field in exercise.Fields)
        {
            if (!entry.Errors.ContainsKey(field.Key)
                && codes.TryGetValue(field.Metric, out var code)
                && Message(exercise, field, code) is { } message)
            {
                entry.Errors[field.Key] = message;
            }
        }

        LogUnworded(exercise, entry, logExerciseError ? result.Error : null, codes);
        var valid = entry.Errors.Count == 0 && result.IsValid;
        entry.CheckOnChange |= !valid;
        return valid;
    }

    /// <inheritdoc />
    public WorkoutExercise Read(ExerciseState exercise) => exercise.Kind switch
    {
        ExerciseKind.Strength => new StrengthExercise(exercise.ExerciseId, exercise.Entries.Select(e => ToSet(Values(exercise, e, null))).ToList()),
        ExerciseKind.Cardio => new CardioExercise(exercise.ExerciseId,
            ToEntry(exercise.Entries.FirstOrDefault() is { } entry ? Values(exercise, entry, null) : [])),
        _ => throw new ArgumentOutOfRangeException(nameof(exercise), exercise.Kind, null),
    };

    /// <summary>
    /// The values that could be read, per metric; a blank field reads as its <see cref="FieldSpec.BlankFill"/>.
    /// Adds a format message per unreadable field to <paramref name="errors"/> when given.
    /// </summary>
    private static Dictionary<Metric, decimal> Values(ExerciseState exercise, EntryState entry, Dictionary<string, string>? errors)
    {
        var values = new Dictionary<Metric, decimal>();
        foreach (var field in exercise.Fields)
        {
            var text = entry.Values.GetValueOrDefault(field.Key);
            var (value, error) = FieldReader.Read(field, string.IsNullOrEmpty(text) ? field.BlankFill : text);
            if (error is not null)
            {
                errors?.Add(field.Key, error);
            }
            else if (value is { } read)
            {
                values[field.Metric] = read;
            }
        }

        return values;
    }

    /// <summary>Logs Core codes that gave no message: <paramref name="exerciseError"/>, or a field error on a field without one.</summary>
    private void LogUnworded(ExerciseState exercise, EntryState entry, ExerciseError? exerciseError, IReadOnlyDictionary<Metric, FieldError> codes)
    {
        if (exerciseError is { } error)
        {
            Log.ExerciseErrorUnworded(logger, exercise.ExerciseId, error);
        }

        foreach (var (metric, code) in codes)
        {
            if (!exercise.Fields.Any(f => f.Metric == metric && entry.Errors.ContainsKey(f.Key)))
            {
                Log.FieldErrorUnworded(logger, exercise.ExerciseId, metric, code);
            }
        }
    }

    /// <summary>Values too large for the set's types are capped (reps at <see cref="int.MaxValue"/>, duration just over the maximum), so Core reports them as too large.</summary>
    private static StrengthSet ToSet(Dictionary<Metric, decimal> values) => new(
        Whole(values, Metric.Reps),
        Get(values, Metric.WeightKg),
        Time(values, Metric.Duration, MaxDurationSeconds),
        Get(values, Metric.Distance));

    /// <summary>Capped as <see cref="ToSet"/> is.</summary>
    private static CardioEntry ToEntry(Dictionary<Metric, decimal> values) => new(
        Time(values, Metric.Duration, MaxDurationSeconds),
        Get(values, Metric.Distance),
        Whole(values, Metric.Floors),
        Whole(values, Metric.Level),
        Get(values, Metric.Incline),
        Whole(values, Metric.Calories),
        Get(values, Metric.Rate));

    private static decimal? Get(Dictionary<Metric, decimal> values, Metric metric) => values.TryGetValue(metric, out var value) ? value : null;

    private static int? Whole(Dictionary<Metric, decimal> values, Metric metric) =>
        Get(values, metric) is { } value ? (int)Math.Min(value, int.MaxValue) : null;

    private static TimeSpan? Time(Dictionary<Metric, decimal> values, Metric metric, decimal maxSeconds) =>
        Get(values, metric) is { } seconds ? TimeSpan.FromSeconds((double)Math.Min(seconds, maxSeconds + 1)) : null;

    /// <summary>Message for a Core code. <see cref="FieldError.NotShown"/> and <see cref="FieldError.TooManyDecimals"/> can't come from the UI and have none.</summary>
    private string? Message(ExerciseState exercise, FieldSpec field, FieldError code) => code switch
    {
        FieldError.Missing => $"Enter {field.Label.ToLowerInvariant()}",
        FieldError.NotPositive => $"{field.Label} must be more than 0",
        FieldError.Negative => $"{field.Label} must be 0 or more",
        FieldError.NotWholeSeconds => $"{field.Label} must be whole seconds",
        FieldError.TooLarge => $"{field.Label} must be {MaxText(exercise, field)} or less",
        _ => null,
    };

    /// <summary>The maximum as the field is entered: h:mm:ss for a duration, otherwise a number.</summary>
    private string MaxText(ExerciseState exercise, FieldSpec field)
    {
        var max = MetricLimits.Max(catalog.All.First(e => e.Id == exercise.ExerciseId), field.Metric);
        return field.Kind == FieldKind.Duration
            ? TimeSpan.FromSeconds((double)max).ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture)
            : max.ToString("#,0", CultureInfo.InvariantCulture);
    }

    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.SetCheckExerciseError, LogLevel.Warning, "Set check for exercise {ExerciseId} failed with {Error}")]
        public static partial void ExerciseErrorUnworded(ILogger logger, int exerciseId, ExerciseError error);

        [LoggerMessage((int)UiEvent.SetCheckFieldUnworded, LogLevel.Warning, "Set check for exercise {ExerciseId} gave {Error} for {Metric} and no message")]
        public static partial void FieldErrorUnworded(ILogger logger, int exerciseId, Metric metric, FieldError error);
    }
}
