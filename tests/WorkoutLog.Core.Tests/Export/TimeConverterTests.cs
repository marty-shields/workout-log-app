using System.Text.Json;
using WorkoutLog.Core.Export;

namespace WorkoutLog.Core.Tests.Export;

public class TimeConverterTests
{
    private static readonly JsonSerializerOptions Options = new() { Converters = { new TimeConverter() } };

    [TestCase("\"00:00:00\"", 0, 0, 0)]
    [TestCase("\"01:02:03\"", 1, 2, 3)]
    [TestCase("\"23:59:59\"", 23, 59, 59)]
    [TestCase("\"100:00:00\"", 100, 0, 0)]
    [TestCase("\"007:00:00\"", 7, 0, 0)]
    public void Read_HoursWithTwoOrMoreDigits_IsTheDuration(string json, int hours, int minutes, int seconds) =>
        Assert.That(JsonSerializer.Deserialize<TimeSpan>(json, Options), Is.EqualTo(new TimeSpan(hours, minutes, seconds)));

    [TestCase("\"1:02:03\"")]
    [TestCase("\"01:2:03\"")]
    [TestCase("\"01:02:3\"")]
    [TestCase("\"01:60:00\"")]
    [TestCase("\"01:00:60\"")]
    [TestCase("\"01:02\"")]
    [TestCase("\"01:02:03:04\"")]
    [TestCase("\"01:02:03.5\"")]
    [TestCase("\"-1:02:03\"")]
    [TestCase("\" 01:02:03\"")]
    [TestCase("\"01:02:03\\n\"")]
    [TestCase("\"\"")]
    [TestCase("\"ab:cd:ef\"")]
    [TestCase("3600")]
    [TestCase("null")]
    public void Read_AnythingElse_ThrowsJsonException(string json) =>
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TimeSpan>(json, Options));

    [TestCase("\"300000000:00:00\"")]
    [TestCase("\"256204779:00:00\"")]
    [TestCase("\"256204778:59:59\"")]
    [TestCase("\"99999999999999999999:00:00\"")]
    public void Read_HoursBeyondATimeSpan_ThrowsJsonException(string json) =>
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TimeSpan>(json, Options));

    [Test]
    public void Read_LargestHoursThatFit_IsTheDuration() =>
        Assert.That(JsonSerializer.Deserialize<TimeSpan>("\"256204777:59:59\"", Options), Is.EqualTo(new TimeSpan(256204777, 59, 59)));

    [TestCase(0, 0, 0, "\"00:00:00\"")]
    [TestCase(1, 2, 3, "\"01:02:03\"")]
    [TestCase(100, 5, 9, "\"100:05:09\"")]
    public void Write_PadsEachPartToTwoDigits(int hours, int minutes, int seconds, string expected) =>
        Assert.That(JsonSerializer.Serialize(new TimeSpan(hours, minutes, seconds), Options), Is.EqualTo(expected));

    [Test]
    public void Write_DropsFractionsOfASecond() =>
        Assert.That(JsonSerializer.Serialize(new TimeSpan(0, 0, 0, 1, 900), Options), Is.EqualTo("\"00:00:01\""));

    [Test]
    public void Write_DaysCountAsHours() =>
        Assert.That(JsonSerializer.Serialize(TimeSpan.FromDays(2), Options), Is.EqualTo("\"48:00:00\""));
}
