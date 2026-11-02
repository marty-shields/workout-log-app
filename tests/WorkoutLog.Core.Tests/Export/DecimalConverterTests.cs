using System.Text.Json;
using WorkoutLog.Core.Export;

namespace WorkoutLog.Core.Tests.Export;

public class DecimalConverterTests
{
    private static readonly JsonSerializerOptions Options = new() { Converters = { new DecimalConverter() } };

    [TestCase("12", "12")]
    [TestCase("12.5", "12.5")]
    [TestCase("12.50", "12.5")]
    [TestCase("0", "0")]
    [TestCase("-3.25", "-3.25")]
    [TestCase("1e2", "100")]
    public void Read_Number_IsTheNumber(string json, string expected) =>
        Assert.That(JsonSerializer.Deserialize<decimal>(json, Options), Is.EqualTo(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture)));

    [Test]
    public void Read_ThreeOrMoreDecimalPlaces_KeepsThemAll() =>
        Assert.That(JsonSerializer.Deserialize<decimal>("1.2345", Options), Is.EqualTo(1.2345m));

    [TestCase("\"12\"")]
    [TestCase("\"12.5\"")]
    [TestCase("\"\"")]
    [TestCase("true")]
    [TestCase("null")]
    [TestCase("[1]")]
    [TestCase("1e40")]
    public void Read_NotANumberOrOutOfRange_ThrowsJsonException(string json) =>
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<decimal>(json, Options));

    [TestCase(12, "12")]
    [TestCase(12.5, "12.5")]
    [TestCase(12.50, "12.5")]
    [TestCase(0, "0")]
    [TestCase(-3.25, "-3.25")]
    public void Write_DropsTrailingZeros(double value, string expected) =>
        Assert.That(JsonSerializer.Serialize((decimal)value, Options), Is.EqualTo(expected));

    [TestCase("1.234", "1.23")]
    [TestCase("1.235", "1.24")]
    [TestCase("2.345", "2.35")]
    [TestCase("0.001", "0")]
    public void Write_RoundsToTwoDecimalPlaces(string value, string expected) =>
        Assert.That(JsonSerializer.Serialize(decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture), Options), Is.EqualTo(expected));
}
