using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Logging;
using WorkoutLog.UI.Models.Fields;

namespace WorkoutLog.UI.Models.Workout;

/// <summary>
/// Reads each field's text, then checks the set or entry with <see cref="IWorkoutService.ValidateSet"/> or
/// <see cref="IWorkoutService.ValidateEntry"/> and words its codes.
/// A field that can't be read gets a format message and no message from Core.
/// A Core code with no message, which the UI can't cause, is logged and fails the set or entry.
/// </summary>
public sealed partial class SetChecker(IWorkoutService workouts, IExerciseCatalog catalog, ILogger<SetChecker> logger) : ISetChecker
{
    private const int MaxDecimalPlaces = 2;
    private static readonly decimal MaxDurationSeconds = (decimal)MetricLimits.MaxDuration.TotalSeconds;
    private static readonly decimal MaxPaceSeconds = (decimal)MetricLimits.MaxPace.TotalSeconds;

    /// <inheritdoc />
    public bool Check(ExerciseState exercise, EntryState entry) => Check(exercise, entry, logExerciseError: true);

    /// <inheritdoc />
    public bool CheckOnSave(ExerciseState exercise, EntryState entry) => Check(exercise, entry, logExerciseError: false);

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

    /// <summary>The values that could be read, per metric. Adds a format message per unreadable field to <paramref name="errors"/> when given.</summary>
    private static Dictionary<Metric, decimal> Values(ExerciseState exercise, EntryState entry, Dictionary<string, string>? errors)
    {
        var values = new Dictionary<Metric, decimal>();
        foreach (var field in exercise.Fields)
        {
            var (value, error) = ReadField(field, entry.Values.GetValueOrDefault(field.Key));
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

    /// <summary>The field's value (a duration or pace in seconds), or null when empty; or a format message.</summary>
    private static (decimal? Value, string? Error) ReadField(FieldSpec field, string? text) => field.Kind switch
    {
        FieldKind.Whole or FieldKind.Decimal => ReadNumber(field, text),
        FieldKind.Duration or FieldKind.Pace => ReadTime(field, text),
        _ => throw new ArgumentOutOfRangeException(nameof(field), field.Kind, "Unknown field kind"),
    };

    private static (decimal? Value, string? Error) ReadNumber(FieldSpec field, string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return (null, null);
        }

        if (!NumberText().IsMatch(text))
        {
            return (null, $"{field.Label} must be a number");
        }

        var point = text.IndexOf('.');
        if (field.Kind == FieldKind.Whole && point >= 0)
        {
            return (null, $"{field.Label} must be a whole number");
        }

        if (point >= 0 && text.Length - point - 1 > MaxDecimalPlaces)
        {
            return (null, $"{field.Label} can have up to {MaxDecimalPlaces} decimal places");
        }

        // Only too many digits fails here; decimal.MaxValue makes Core report it as too large.
        return (decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value) ? value : decimal.MaxValue, null);
    }

    /// <summary>Reads h:mm:ss (duration) or m:ss (pace). Empty boxes count as 0; all empty is an empty field.</summary>
    private static (decimal? Value, string? Error) ReadTime(FieldSpec field, string? text)
    {
        var pace = field.Kind == FieldKind.Pace;
        var parts = (text ?? "").Split(':');
        if (parts.All(string.IsNullOrEmpty))
        {
            return (null, null);
        }

        if (parts.Length != (pace ? 2 : 3))
        {
            return (null, $"{field.Label} must be a time");
        }

        var numbers = new int[parts.Length];
        for (var i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length > 0
                && !(DigitsText().IsMatch(parts[i]) && int.TryParse(parts[i], NumberStyles.None, CultureInfo.InvariantCulture, out numbers[i])))
            {
                return (null, $"{field.Label} must be a time");
            }
        }

        if (pace)
        {
            return numbers[1] > 59
                ? (null, $"{field.Label} seconds must be 00–59")
                : ((decimal)numbers[0] * 60 + numbers[1], null);
        }

        if (numbers[1] > 59 || numbers[2] > 59)
        {
            return (null, $"{field.Label} minutes and seconds must be 00–59");
        }

        return ((decimal)numbers[0] * 3_600 + numbers[1] * 60 + numbers[2], null);
    }

    /// <summary>Values too large for the set's types are capped (reps at <see cref="int.MaxValue"/>, duration just over the maximum), so Core reports them as too large.</summary>
    private static StrengthSet ToSet(Dictionary<Metric, decimal> values) => new(
        Whole(values, Metric.Reps),
        Get(values, Metric.WeightKg),
        Time(values, Metric.Duration, MaxDurationSeconds),
        Get(values, Metric.Distance));

    /// <summary>Capped as <see cref="ToSet"/> is; pace just over its maximum.</summary>
    private static CardioEntry ToEntry(Dictionary<Metric, decimal> values) => new(
        Time(values, Metric.Duration, MaxDurationSeconds),
        Get(values, Metric.Distance),
        Whole(values, Metric.Floors),
        Whole(values, Metric.Level),
        Get(values, Metric.Incline),
        Whole(values, Metric.Calories),
        Get(values, Metric.Rate),
        Time(values, Metric.Pace, MaxPaceSeconds));

    private static decimal? Get(Dictionary<Metric, decimal> values, Metric metric) => values.TryGetValue(metric, out var value) ? value : null;

    private static int? Whole(Dictionary<Metric, decimal> values, Metric metric) =>
        Get(values, metric) is { } value ? (int)Math.Min(value, int.MaxValue) : null;

    private static TimeSpan? Time(Dictionary<Metric, decimal> values, Metric metric, decimal maxSeconds) =>
        Get(values, metric) is { } seconds ? TimeSpan.FromSeconds((double)Math.Min(seconds, maxSeconds + 1)) : null;

    /// <summary>Message for a Core code. <see cref="FieldError.NotShown"/> can't come from the UI and has none.</summary>
    private string? Message(ExerciseState exercise, FieldSpec field, FieldError code) => code switch
    {
        FieldError.Missing => $"Enter {field.Label.ToLowerInvariant()}",
        FieldError.NotPositive => $"{field.Label} must be more than 0",
        FieldError.Negative => $"{field.Label} must be 0 or more",
        FieldError.NotWholeSeconds => $"{field.Label} must be whole seconds",
        FieldError.TooLarge => $"{field.Label} must be {MaxText(exercise, field)} or less",
        _ => null,
    };

    /// <summary>The maximum as the field is entered: h:mm:ss for a duration, m:ss for a pace, otherwise a number.</summary>
    private string MaxText(ExerciseState exercise, FieldSpec field)
    {
        var max = MetricLimits.Max(catalog.All.First(e => e.Id == exercise.ExerciseId), field.Metric);
        return field.Kind switch
        {
            FieldKind.Duration => TimeSpan.FromSeconds((double)max).ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture),
            FieldKind.Pace => $"{(int)(max / 60)}:{(int)(max % 60):00}",
            _ => max.ToString("#,0", CultureInfo.InvariantCulture),
        };
    }

    /// <summary>Digits with an optional point and digits after it, or a point and digits (.5).</summary>
    [GeneratedRegex(@"^([0-9]+(\.[0-9]+)?|\.[0-9]+)\z")]
    private static partial Regex NumberText();

    [GeneratedRegex(@"^[0-9]+\z")]
    private static partial Regex DigitsText();

    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.SetCheckExerciseError, LogLevel.Warning, "Set check for exercise {ExerciseId} failed with {Error}")]
        public static partial void ExerciseErrorUnworded(ILogger logger, int exerciseId, ExerciseError error);

        [LoggerMessage((int)UiEvent.SetCheckFieldUnworded, LogLevel.Warning, "Set check for exercise {ExerciseId} gave {Error} for {Metric} and no message")]
        public static partial void FieldErrorUnworded(ILogger logger, int exerciseId, Metric metric, FieldError error);
    }
}
