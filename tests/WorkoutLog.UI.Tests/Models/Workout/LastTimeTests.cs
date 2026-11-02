using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Tests.Models.Workout;

public class LastTimeTests
{
    private static readonly LastTime Last = new(["60×8", "1:30", "32kg · 40m"]);

    [TestCase(1, "60×8")]
    [TestCase(2, "1:30")]
    [TestCase(3, "32kg · 40m")]
    public void Entry_ReturnsTheTextOfThatSet(int number, string text) =>
        Assert.That(Last.Entry(number), Is.EqualTo(text));

    [TestCase(4)]
    [TestCase(10)]
    public void Entry_PastTheLastSet_IsADash(int number) =>
        Assert.That(Last.Entry(number), Is.EqualTo("—"));
}
