using System.Text.Json;
using WorkoutLog.Core.Export;

namespace WorkoutLog.Core.Tests.Export;

public class UtcConverterTests
{
    private static readonly JsonSerializerOptions TimeOptions = new() { Converters = { new UtcTimeConverter() } };
    private static readonly JsonSerializerOptions SecondsOptions = new() { Converters = { new UtcSecondsConverter() } };

    private static readonly DateTimeOffset Utc = new(2026, 3, 4, 5, 6, 7, TimeSpan.Zero);
    private static readonly DateTimeOffset Plus2 = new(2026, 3, 4, 7, 6, 7, TimeSpan.FromHours(2));
    private static readonly DateTimeOffset Minus5Fraction = new DateTimeOffset(2026, 3, 3, 23, 6, 7, TimeSpan.FromHours(-5)).AddTicks(1234567);

    [Test]
    public void UtcTime_Utc_HasSevenFractionalDigitsAndZ() =>
        Assert.That(JsonSerializer.Serialize(Utc, TimeOptions), Is.EqualTo("\"2026-03-04T05:06:07.0000000Z\""));

    [Test]
    public void UtcTime_NonUtcOffset_IsWrittenInUtc() =>
        Assert.That(JsonSerializer.Serialize(Plus2, TimeOptions), Is.EqualTo("\"2026-03-04T05:06:07.0000000Z\""));

    [Test]
    public void UtcTime_Fraction_KeepsEveryTick() =>
        Assert.That(JsonSerializer.Serialize(Minus5Fraction, TimeOptions), Is.EqualTo("\"2026-03-04T04:06:07.1234567Z\""));

    [Test]
    public void UtcTime_Read_IsNotSupported() =>
        Assert.Throws<NotSupportedException>(() => JsonSerializer.Deserialize<DateTimeOffset>("\"2026-03-04T05:06:07.0000000Z\"", TimeOptions));

    [Test]
    public void UtcSeconds_Utc_EndsWithZ() =>
        Assert.That(JsonSerializer.Serialize(Utc, SecondsOptions), Is.EqualTo("\"2026-03-04T05:06:07Z\""));

    [Test]
    public void UtcSeconds_NonUtcOffset_IsWrittenInUtc() =>
        Assert.That(JsonSerializer.Serialize(Plus2, SecondsOptions), Is.EqualTo("\"2026-03-04T05:06:07Z\""));

    [Test]
    public void UtcSeconds_Fraction_IsDropped() =>
        Assert.That(JsonSerializer.Serialize(Minus5Fraction, SecondsOptions), Is.EqualTo("\"2026-03-04T04:06:07Z\""));

    [Test]
    public void UtcSeconds_Read_IsNotSupported() =>
        Assert.Throws<NotSupportedException>(() => JsonSerializer.Deserialize<DateTimeOffset>("\"2026-03-04T05:06:07Z\"", SecondsOptions));
}
