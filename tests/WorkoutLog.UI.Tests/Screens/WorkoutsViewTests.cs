using WorkoutLog.UI.Models.Workouts;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class WorkoutsViewTests : ComponentTestBase
{
    private static readonly WorkoutsRow[] Rows =
    [
        new(2, new DateTime(2026, 10, 20, 18, 5, 0)),
        new(1, new DateTime(2026, 10, 20, 7, 30, 0)),
    ];

    [Test]
    public void Rows_ShowDayDateAndStartTimeInGivenOrder()
    {
        var cut = Context.Render<WorkoutsView>(p => p.Add(x => x.Rows, Rows));

        Assert.That(cut.FindAll(".text").Select(t => t.TextContent),
            Is.EqualTo(new[] { "Tue 20 Oct 2026 · 18:05", "Tue 20 Oct 2026 · 07:30" }));
    }

    [Test]
    public void NoRows_ShowsNoWorkoutsYet()
    {
        var cut = Context.Render<WorkoutsView>(p => p.Add(x => x.Rows, []));

        Assert.That(cut.Find("[data-testid=workouts-empty]").TextContent, Does.Contain("No workouts yet"));
    }

    [Test]
    public void WithOnOpen_TappingRowRaisesIt()
    {
        WorkoutsRow? opened = null;
        var cut = Context.Render<WorkoutsView>(p => p
            .Add(x => x.Rows, Rows)
            .Add(x => x.OnOpen, (WorkoutsRow row) => opened = row));

        cut.Find("[data-testid=workouts-row-1]").Click();

        Assert.That(opened, Is.EqualTo(Rows[1]));
    }

    [Test]
    public void Loading_ShowsTheSpinnerOnly()
    {
        var cut = Context.Render<WorkoutsView>(p => p.Add(x => x.Rows, Rows).Add(x => x.Loading, true).Add(x => x.OnBack, () => { }));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=workouts-loading]").TextContent, Does.Contain("Loading…"));
            Assert.That(cut.FindAll(".rows"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=workouts-empty]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=workouts-error]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=back]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Failed_ShowsTheErrorAndTryAgainInsteadOfTheRows()
    {
        var cut = Context.Render<WorkoutsView>(p => p.Add(x => x.Rows, []).Add(x => x.Failed, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=workouts-error]").TextContent, Is.EqualTo("Couldn't load your workouts"));
            Assert.That(cut.Find("[data-testid=workouts-error]").GetAttribute("role"), Is.EqualTo("alert"));
            Assert.That(cut.Find("[data-testid=workouts-retry]").TextContent.Trim(), Is.EqualTo("Try again"));
            Assert.That(cut.FindAll("[data-testid=workouts-empty]"), Is.Empty);
        });
    }

    [Test]
    public void TryAgain_RaisesOnRetry()
    {
        var retries = 0;
        var cut = Context.Render<WorkoutsView>(p => p
            .Add(x => x.Rows, [])
            .Add(x => x.Failed, true)
            .Add(x => x.OnRetry, () => retries++));

        cut.Find("[data-testid=workouts-retry]").Click();

        Assert.That(retries, Is.EqualTo(1));
    }

    [Test]
    public void Failed_FocusesTryAgain()
    {
        var cut = Context.Render<WorkoutsView>(p => p.Add(x => x.Rows, []).Add(x => x.Failed, true));

        cut.WaitForAssertion(() => Focuses().Single().Arguments[0]
            .ShouldBeElementReferenceTo(cut.Find("[data-testid=workouts-retry]")));
    }

    [Test]
    public void Failed_RenderedAgain_DoesNotFocusAgain()
    {
        var cut = Context.Render<WorkoutsView>(p => p.Add(x => x.Rows, []).Add(x => x.Failed, true));
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Render(p => p.Add(x => x.Failed, true));

        Assert.That(Focuses(), Has.Count.EqualTo(1));
    }

    [Test]
    public void FailedAfterLoading_FocusesTryAgainAgain()
    {
        var cut = Context.Render<WorkoutsView>(p => p.Add(x => x.Rows, []).Add(x => x.Failed, true));
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Render(p => p.Add(x => x.Loading, true));
        cut.Render(p => p.Add(x => x.Loading, false));

        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(2)));
    }

    [Test]
    public void Back_RaisesOnBack()
    {
        var back = false;
        var cut = Context.Render<WorkoutsView>(p => p
            .Add(x => x.Rows, Rows)
            .Add(x => x.OnBack, () => back = true));

        cut.Find("[data-testid=back]").Click();

        Assert.That(back, Is.True);
    }

    private IReadOnlyList<JSRuntimeInvocation> Focuses() =>
        Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"];
}
