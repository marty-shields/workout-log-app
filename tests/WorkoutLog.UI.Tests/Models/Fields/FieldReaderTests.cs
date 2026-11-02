using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Models.Fields;

namespace WorkoutLog.UI.Tests.Models.Fields;

public class FieldReaderTests
{
    private static readonly FieldSpec Reps = new("reps", Metric.Reps, "Reps", FieldKind.Whole);
    private static readonly FieldSpec Weight = new("weight", Metric.WeightKg, "Weight", FieldKind.Decimal);
    private static readonly FieldSpec Time = new("time", Metric.Duration, "Time", FieldKind.Duration);

    [TestCase(null)]
    [TestCase("")]
    public void Read_EmptyNumber_IsEmptyWithNoError(string? text)
    {
        Assert.That(FieldReader.Read(Reps, text), Is.EqualTo(((decimal?)null, (string?)null)));
        Assert.That(FieldReader.Read(Weight, text), Is.EqualTo(((decimal?)null, (string?)null)));
    }

    [TestCase("0", 0)]
    [TestCase("12", 12)]
    [TestCase("007", 7)]
    public void Read_Whole_IsTheNumber(string text, int expected) =>
        Assert.That(FieldReader.Read(Reps, text), Is.EqualTo(((decimal?)expected, (string?)null)));

    [TestCase("12.5")]
    [TestCase("12.")]
    [TestCase(".5")]
    public void Read_WholeWithAPoint_MustBeWhole(string text)
    {
        var (value, error) = FieldReader.Read(Reps, text);

        Assert.That(value, Is.Null);
        Assert.That(error, Is.AnyOf("Reps must be a whole number", "Reps must be a number"));
    }

    [TestCase("12.5", 12.5)]
    [TestCase(".5", 0.5)]
    [TestCase("12.25", 12.25)]
    [TestCase("12", 12)]
    public void Read_Decimal_IsTheNumber(string text, double expected) =>
        Assert.That(FieldReader.Read(Weight, text), Is.EqualTo(((decimal?)(decimal)expected, (string?)null)));

    [TestCase("abc")]
    [TestCase("1,5")]
    [TestCase("-1")]
    [TestCase("+1")]
    [TestCase("1e3")]
    [TestCase("1.2.3")]
    [TestCase("12.")]
    [TestCase(" 1")]
    [TestCase("1\n")]
    public void Read_NotANumber_HasAMessage(string text) =>
        Assert.That(FieldReader.Read(Weight, text), Is.EqualTo(((decimal?)null, (string?)"Weight must be a number")));

    [Test]
    public void Read_TooManyDecimalPlaces_HasAMessage() =>
        Assert.That(FieldReader.Read(Weight, "1.234"), Is.EqualTo(((decimal?)null, (string?)"Weight can have up to 2 decimal places")));

    [Test]
    public void Read_TooManyDigits_IsDecimalMaxValueForCoreToRejectAsTooLarge() =>
        Assert.That(FieldReader.Read(Reps, new string('9', 40)), Is.EqualTo(((decimal?)decimal.MaxValue, (string?)null)));

    [TestCase(null)]
    [TestCase("")]
    [TestCase("::")]
    public void Read_EmptyDuration_IsEmptyWithNoError(string? text) =>
        Assert.That(FieldReader.Read(Time, text), Is.EqualTo(((decimal?)null, (string?)null)));

    [TestCase("1:02:03", 3723)]
    [TestCase("0:00:00", 0)]
    [TestCase("23:59:59", 86399)]
    [TestCase("::5", 5)]
    [TestCase(":2:", 120)]
    [TestCase("1::", 3600)]
    [TestCase("1:2:3", 3723)]
    public void Read_Duration_IsSeconds_AndEmptyBoxesCountAsZero(string text, int seconds) =>
        Assert.That(FieldReader.Read(Time, text), Is.EqualTo(((decimal?)seconds, (string?)null)));

    [TestCase("1")]
    [TestCase("1:2")]
    [TestCase("1:2:3:4")]
    [TestCase("a:00:00")]
    [TestCase("-1:00:00")]
    [TestCase("1.5:00:00")]
    [TestCase("99999999999:00:00")]
    public void Read_BadDuration_MustBeATime(string text) =>
        Assert.That(FieldReader.Read(Time, text), Is.EqualTo(((decimal?)null, (string?)"Time must be a time")));

    [TestCase("0:60:00")]
    [TestCase("0:00:60")]
    public void Read_MinutesOrSecondsOver59_HasAMessage(string text) =>
        Assert.That(FieldReader.Read(Time, text), Is.EqualTo(((decimal?)null, (string?)"Time minutes and seconds must be 00–59")));

    [Test]
    public void Read_UnknownKind_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => FieldReader.Read(Reps with { Kind = (FieldKind)99 }, "1"));
}
