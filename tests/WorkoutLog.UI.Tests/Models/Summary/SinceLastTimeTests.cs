using WorkoutLog.UI.Models.Summary;

namespace WorkoutLog.UI.Tests.Models.Summary;

public class SinceLastTimeTests
{
    [Test]
    public void Up_IsUpWithItsDifference()
    {
        var since = SinceLastTime.Up("2.5kg");

        Assert.That((since.Kind, since.Difference), Is.EqualTo((ChangeKind.Up, "2.5kg")));
    }

    [Test]
    public void Down_IsDownWithItsDifference()
    {
        var since = SinceLastTime.Down("2kg");

        Assert.That((since.Kind, since.Difference), Is.EqualTo((ChangeKind.Down, "2kg")));
    }

    [Test]
    public void EqualAndFirstTime_HaveNoDifference()
    {
        Assert.Multiple(() =>
        {
            Assert.That((SinceLastTime.Equal.Kind, SinceLastTime.Equal.Difference), Is.EqualTo((ChangeKind.Equal, (string?)null)));
            Assert.That((SinceLastTime.FirstTime.Kind, SinceLastTime.FirstTime.Difference), Is.EqualTo((ChangeKind.FirstTime, (string?)null)));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void UpAndDown_WithoutADifference_Throw(string? difference)
    {
        Assert.Multiple(() =>
        {
            Assert.That(() => SinceLastTime.Up(difference!), Throws.InstanceOf<ArgumentException>());
            Assert.That(() => SinceLastTime.Down(difference!), Throws.InstanceOf<ArgumentException>());
        });
    }

    [Test]
    public void Values_CompareByKindAndDifference()
    {
        var up = SinceLastTime.Up("5kg");

        Assert.Multiple(() =>
        {
            Assert.That(SinceLastTime.Up("5kg"), Is.EqualTo(up));
            Assert.That(SinceLastTime.Down("5kg"), Is.Not.EqualTo(up));
        });
    }
}
