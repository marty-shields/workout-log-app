using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Models.Fields;

namespace WorkoutLog.UI.Models.Workout;

/// <summary>
/// Reads each field's text, then checks the set with <see cref="IWorkoutService.ValidateSet"/> and words its codes.
/// A field that can't be read gets a format message and no message from Core.
/// A Core code with no message, which the UI can't cause, is logged and fails the set.
/// </summary>
public sealed partial class SetChecker(IWorkoutService workouts, IExerciseCatalog catalog, ILogger<SetChecker> logger) : ISetChecker
{
    private const int MaxDecimalPlaces = 2;
    private static readonly decimal MaxDurationSeconds = (decimal)MetricLimits.MaxDuration.TotalSeconds;

    /// <inheritdoc />
    public bool Check(ExerciseState exercise, EntryState entry)
    {
        entry.Errors.Clear();
        if (exercise.Kind == ExerciseKind.Cardio)
        {
            return true;
        }

        var values = new Dictionary<Metric, decimal>();
        foreach (var field in exercise.Fields)
        {
            var (value, error) = Read(field, entry.Values.GetValueOrDefault(field.Key));
            if (error is not null)
            {
                entry.Errors[field.Key] = error;
            }
            else if (value is { } read)
            {
                values[field.Metric] = read;
            }
        }

        var result = workouts.ValidateSet(exercise.ExerciseId, ToSet(values));
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

        LogUnworded(exercise, entry, result, codes);
        return entry.Errors.Count == 0 && result.IsValid;
    }

    /// <summary>Logs Core codes that gave no message: an exercise error, or a field error on a field without one.</summary>
    private void LogUnworded(ExerciseState exercise, EntryState entry, ExerciseErrors result, IReadOnlyDictionary<Metric, FieldError> codes)
    {
        if (result.Error is { } error)
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

    /// <summary>The field's value (a duration in seconds), or null when empty; or a format message.</summary>
    private static (decimal? Value, string? Error) Read(FieldSpec field, string? text) => field.Kind switch
    {
        FieldKind.Whole or FieldKind.Decimal => ReadNumber(field, text),
        FieldKind.Duration => ReadDuration(field, text),
        _ => throw new ArgumentOutOfRangeException(nameof(field), field.Kind, "Strength sets have no such field"),
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

    /// <summary>Reads h:mm:ss. Empty boxes count as 0; all empty is an empty field.</summary>
    private static (decimal? Value, string? Error) ReadDuration(FieldSpec field, string? text)
    {
        var parts = (text ?? "").Split(':');
        if (parts.All(string.IsNullOrEmpty))
        {
            return (null, null);
        }

        if (parts.Length != 3)
        {
            return (null, $"{field.Label} must be a time");
        }

        var numbers = new int[3];
        for (var i = 0; i < 3; i++)
        {
            if (parts[i].Length > 0
                && !(DigitsText().IsMatch(parts[i]) && int.TryParse(parts[i], NumberStyles.None, CultureInfo.InvariantCulture, out numbers[i])))
            {
                return (null, $"{field.Label} must be a time");
            }
        }

        if (numbers[1] > 59 || numbers[2] > 59)
        {
            return (null, $"{field.Label} minutes and seconds must be 00–59");
        }

        return ((decimal)numbers[0] * 3_600 + numbers[1] * 60 + numbers[2], null);
    }

    /// <summary>Values too large for the set's types are capped (reps at <see cref="int.MaxValue"/>, duration just over the maximum), so Core reports them as too large.</summary>
    private static StrengthSet ToSet(Dictionary<Metric, decimal> values)
    {
        decimal? Get(Metric metric) => values.TryGetValue(metric, out var value) ? value : null;

        return new StrengthSet(
            Get(Metric.Reps) is { } reps ? (int)Math.Min(reps, int.MaxValue) : null,
            Get(Metric.WeightKg),
            Get(Metric.Duration) is { } seconds ? TimeSpan.FromSeconds((double)Math.Min(seconds, MaxDurationSeconds + 1)) : null,
            Get(Metric.Distance));
    }

    /// <summary>Message for a Core code. <see cref="FieldError.NotShown"/> can't come from the UI and has none.</summary>
    private string? Message(ExerciseState exercise, FieldSpec field, FieldError code) => code switch
    {
        FieldError.Missing => $"Enter {field.Label.ToLowerInvariant()}",
        FieldError.NotPositive => $"{field.Label} must be more than 0",
        FieldError.Negative => $"{field.Label} must be 0 or more",
        FieldError.TooLarge => $"{field.Label} must be {MaxText(exercise, field)} or less",
        _ => null,
    };

    private string MaxText(ExerciseState exercise, FieldSpec field) => field.Metric == Metric.Duration
        ? MetricLimits.MaxDuration.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture)
        : MetricLimits.Max(catalog.All.First(e => e.Id == exercise.ExerciseId), field.Metric).ToString("#,0", CultureInfo.InvariantCulture);

    /// <summary>Digits with an optional point and digits after it, or a point and digits (.5).</summary>
    [GeneratedRegex(@"^([0-9]+(\.[0-9]+)?|\.[0-9]+)\z")]
    private static partial Regex NumberText();

    [GeneratedRegex(@"^[0-9]+\z")]
    private static partial Regex DigitsText();

    private static partial class Log
    {
        [LoggerMessage(2501, LogLevel.Warning, "Set check for exercise {ExerciseId} failed with {Error} and no message")]
        public static partial void ExerciseErrorUnworded(ILogger logger, int exerciseId, ExerciseError error);

        [LoggerMessage(2502, LogLevel.Warning, "Set check for exercise {ExerciseId} gave {Error} for {Metric} and no message")]
        public static partial void FieldErrorUnworded(ILogger logger, int exerciseId, Metric metric, FieldError error);
    }
}
