using System.Globalization;
using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Tests.Domain.Exercises;

public class DecimalsTests
{
    [TestCase("22.5", "22.5")]
    [TestCase("22.504", "22.5")]
    [TestCase("22.505", "22.51")]
    [TestCase("22.515", "22.52")]
    [TestCase("62.4999999999", "62.5")]
    [TestCase("-1.005", "-1.01")]
    [TestCase("60", "60")]
    public void Round2_RoundsTo2PlacesHalvesAwayFromZero(string value, string expected) =>
        Assert.That(Decimals.Round2(decimal.Parse(value, CultureInfo.InvariantCulture)), Is.EqualTo(decimal.Parse(expected, CultureInfo.InvariantCulture)));
}
