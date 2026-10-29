using System.Globalization;
using System.Text.RegularExpressions;
using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.UI.Models.Fields;

/// <summary>Reads a field's entered text as a number.</summary>
internal static partial class FieldReader
{
    /// <summary>The field's value (a duration in seconds), or null when empty; or a format message.</summary>
    public static (decimal? Value, string? Error) Read(FieldSpec field, string? text) => field.Kind switch
    {
        FieldKind.Whole or FieldKind.Decimal => ReadNumber(field, text),
        FieldKind.Duration => ReadDuration(field, text),
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

        if (point >= 0 && text.Length - point - 1 > MetricLimits.MaxDecimalPlaces)
        {
            return (null, $"{field.Label} can have up to {MetricLimits.MaxDecimalPlaces} decimal places");
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

        var numbers = new int[parts.Length];
        for (var i = 0; i < parts.Length; i++)
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

    /// <summary>Digits with an optional point and digits after it, or a point and digits (.5).</summary>
    [GeneratedRegex(@"^([0-9]+(\.[0-9]+)?|\.[0-9]+)\z")]
    private static partial Regex NumberText();

    [GeneratedRegex(@"^[0-9]+\z")]
    private static partial Regex DigitsText();
}
