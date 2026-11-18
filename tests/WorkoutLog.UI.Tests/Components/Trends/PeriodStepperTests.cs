using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.UI.Components.Trends;

namespace WorkoutLog.UI.Tests.Components.Trends;

public class PeriodStepperTests : ComponentTestBase
{
    [Test]
    public void ShowsTheLabelBetweenTwoArrows()
    {
        var cut = Render(PeriodKind.Week, canPrevious: true, canNext: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=stepper-label]").TextContent, Is.EqualTo("16 – 22 Nov"));
            Assert.That(cut.Find("[data-testid=stepper-label]").GetAttribute("aria-live"), Is.EqualTo("polite"));
            Assert.That(cut.Find("[data-testid=stepper-previous]").HasAttribute("disabled"), Is.False);
            Assert.That(cut.Find("[data-testid=stepper-next]").HasAttribute("disabled"), Is.False);
        });
    }

    [TestCase(PeriodKind.Week, "Previous week", "Next week")]
    [TestCase(PeriodKind.Month, "Previous month", "Next month")]
    [TestCase(PeriodKind.Year, "Previous year", "Next year")]
    public void Arrows_AreNamedAfterTheKind(PeriodKind kind, string previous, string next)
    {
        var cut = Render(kind, canPrevious: true, canNext: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=stepper-previous]").GetAttribute("aria-label"), Is.EqualTo(previous));
            Assert.That(cut.Find("[data-testid=stepper-next]").GetAttribute("aria-label"), Is.EqualTo(next));
        });
    }

    [Test]
    public void AtTheLatestPeriod_TheNextArrowIsDisabled()
    {
        var cut = Render(PeriodKind.Month, canPrevious: true, canNext: false);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=stepper-next]").HasAttribute("disabled"), Is.True);
            Assert.That(cut.Find("[data-testid=stepper-previous]").HasAttribute("disabled"), Is.False);
        });
    }

    [Test]
    public void AtTheEarliestPeriod_ThePreviousArrowIsDisabled()
    {
        var cut = Render(PeriodKind.Year, canPrevious: false, canNext: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=stepper-previous]").HasAttribute("disabled"), Is.True);
            Assert.That(cut.Find("[data-testid=stepper-next]").HasAttribute("disabled"), Is.False);
        });
    }

    [Test]
    public void Arrows_RaiseTheirCallbacks()
    {
        var calls = new List<string>();
        var cut = Render(PeriodKind.Week, canPrevious: true, canNext: true, () => calls.Add("next"), () => calls.Add("previous"));

        cut.Find("[data-testid=stepper-previous]").Click();
        cut.Find("[data-testid=stepper-next]").Click();

        Assert.That(calls, Is.EqualTo(new[] { "previous", "next" }));
    }

    private IRenderedComponent<PeriodStepper> Render(PeriodKind kind, bool canPrevious, bool canNext, Action? onNext = null, Action? onPrevious = null) =>
        Context.Render<PeriodStepper>(p => p
            .Add(x => x.Kind, kind)
            .Add(x => x.Label, "16 – 22 Nov")
            .Add(x => x.CanGoPrevious, canPrevious)
            .Add(x => x.CanGoNext, canNext)
            .Add(x => x.OnNext, onNext ?? (() => { }))
            .Add(x => x.OnPrevious, onPrevious ?? (() => { }))
            .Add(x => x.TestId, "stepper"));
}
