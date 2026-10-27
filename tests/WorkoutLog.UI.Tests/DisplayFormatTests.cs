using System.Globalization;

namespace WorkoutLog.UI.Tests;

public class DisplayFormatTests
{
    [TestCase("22.5", "22.5kg")]
    [TestCase("60.00", "60kg")]
    [TestCase("62.4999999999", "62.5kg")]
    [TestCase("22.505", "22.51kg")]
    public void Weight_RoundedTo2PlacesWithoutTrailingZeros(string kg, string expected) =>
        Assert.That(DisplayFormat.Weight(decimal.Parse(kg, CultureInfo.InvariantCulture)), Is.EqualTo(expected));

    [Test]
    public void Weight_None_IsADash() => Assert.That(DisplayFormat.Weight(null), Is.EqualTo("—"));
}
