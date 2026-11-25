using Shields.UI.Components.Feedback;
using WorkoutLog.UI.Screens;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Screens;

/// <summary>The banner for a workout left unfinished, and the dialog that goes with it.</summary>
public class ThisWeekViewUnfinishedTests : ComponentTestBase
{
    [Test]
    public void Unfinished_ShowsTheBannerAtTheTopWithTheAgeCountAndTwoActions()
    {
        var cut = Render();

        var banner = cut.FindComponent<Banner>();
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=unfinished-title]").TextContent, Is.EqualTo("Workout in progress"));
            Assert.That(cut.Find("[data-testid=unfinished-details]").TextContent, Is.EqualTo("Started yesterday, 6:12 pm · 4 exercises"));
            Assert.That(cut.Find("[data-testid=unfinished-resume]").TextContent.Trim(), Is.EqualTo("Resume"));
            Assert.That(cut.Find("[data-testid=unfinished-discard]").TextContent.Trim(), Is.EqualTo("Discard"));
            Assert.That(banner.Find("[data-testid=unfinished-actions]").Children.Select(b => b.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "unfinished-resume", "unfinished-discard" }));
            Assert.That(cut.FindAll("[data-testid=unfinished], [data-testid=this-week-dates]").Select(e => e.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "unfinished", "this-week-dates" }), "the banner is above the week");
        });
    }

    [Test]
    public void NoUnfinished_ShowsNoBanner()
    {
        var cut = Context.Render<ThisWeekView>(p => p.Add(x => x.Monday, SampleThisWeek.Monday).Add(x => x.Today, SampleThisWeek.Wednesday));

        Assert.That(cut.FindAll("[data-testid=unfinished]"), Is.Empty);
    }

    [Test]
    public void Unfinished_WhileLoading_ShowsNoBanner()
    {
        var cut = Render(p => p.Add(x => x.Loading, true));

        Assert.That(cut.FindAll("[data-testid=unfinished]"), Is.Empty);
    }

    [Test]
    public void Unfinished_WhenTheWeekFailedToLoad_StillShowsTheBanner()
    {
        var cut = Render(p => p.Add(x => x.Failed, true));

        Assert.That(cut.FindAll("[data-testid=unfinished]"), Has.Count.EqualTo(1));
    }

    [Test]
    public void Buttons_RaiseResumeAndDiscard()
    {
        var resumed = 0;
        var discarded = 0;
        var cut = Render(p => p.Add(x => x.OnResume, () => resumed++).Add(x => x.OnDiscardUnfinished, () => discarded++));

        cut.Find("[data-testid=unfinished-resume]").Click();
        cut.Find("[data-testid=unfinished-discard]").Click();

        Assert.That((resumed, discarded), Is.EqualTo((1, 1)));
    }

    [Test]
    public void ConfirmingDiscard_AsksWhatIsLostWithDangerDiscardAndCancel()
    {
        var confirmed = 0;
        var cancelled = 0;
        var cut = Render(p => p.Add(x => x.ConfirmingDiscard, true).Add(x => x.OnConfirmDiscard, () => confirmed++).Add(x => x.OnCancelDiscard, () => cancelled++));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=confirm-dialog] h2").TextContent, Is.EqualTo("Discard this workout?"));
            Assert.That(cut.Find("[data-testid=confirm-dialog] .message").TextContent, Is.EqualTo("4 exercises, 12 sets will be lost"));
            Assert.That(cut.Find("[data-testid=confirm-dialog-confirm]").TextContent.Trim(), Is.EqualTo("Discard"));
            Assert.That(cut.Find("[data-testid=confirm-dialog-confirm]").ClassList, Does.Contain("danger"));
        });
        cut.Find("[data-testid=confirm-dialog-confirm]").Click();
        cut.Find("[data-testid=confirm-dialog-cancel]").Click();
        Assert.That((confirmed, cancelled), Is.EqualTo((1, 1)));
    }

    [Test]
    public void NotConfirmingDiscard_ShowsNoDialog()
    {
        var cut = Render();

        Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
    }

    private IRenderedComponent<ThisWeekView> Render(Action<ComponentParameterCollectionBuilder<ThisWeekView>>? more = null) =>
        Context.Render<ThisWeekView>(p =>
        {
            p.Add(x => x.Monday, SampleThisWeek.Monday)
                .Add(x => x.Today, SampleThisWeek.Wednesday)
                .Add(x => x.Unfinished, SampleThisWeek.Unfinished);
            more?.Invoke(p);
        });
}
