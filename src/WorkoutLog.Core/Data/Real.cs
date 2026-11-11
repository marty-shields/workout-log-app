using System.Globalization;

namespace WorkoutLog.Core.Data;

/// <summary>
/// Reads a REAL column that holds a decimal metric. The double is rounded to 15 significant digits, so the 5.2 the app wrote reads
/// back as 5.2. Since .NET 11 a cast gives the double's exact value instead: 5.20000000000000017763568394.
/// </summary>
internal static class Real
{
    public static decimal ToDecimal(double value) =>
        decimal.Parse(value.ToString("G15", CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture);

    public static decimal? ToDecimal(double? value) => value is { } real ? ToDecimal(real) : null;
}
