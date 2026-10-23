using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.JSInterop;
using WorkoutLog.UI.Logging;
using WorkoutLog.UI.Models.History;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class HistoryViewTests : ComponentTestBase
{
    private static readonly HistoryRow[] Rows =
    [
        new(2, new DateTime(2026, 10, 20, 18, 5, 0)),
        new(1, new DateTime(2026, 10, 20, 7, 30, 0)),
    ];

    [Test]
    public void Rows_ShowDayDateAndStartTimeInGivenOrder()
    {
        var cut = Context.Render<HistoryView>(p => p.Add(x => x.Rows, Rows));

        Assert.That(cut.FindAll(".text").Select(t => t.TextContent),
            Is.EqualTo(new[] { "Tue 20 Oct 2026 · 18:05", "Tue 20 Oct 2026 · 07:30" }));
    }

    [Test]
    public void NoRows_ShowsNoWorkoutsYet()
    {
        var cut = Context.Render<HistoryView>(p => p.Add(x => x.Rows, []));

        Assert.That(cut.Find("[data-testid=history-empty]").TextContent, Does.Contain("No workouts yet"));
    }

    [Test]
    public void WithOnOpen_TappingRowRaisesIt()
    {
        HistoryRow? opened = null;
        var cut = Context.Render<HistoryView>(p => p
            .Add(x => x.Rows, Rows)
            .Add(x => x.OnOpen, (HistoryRow row) => opened = row));

        cut.Find("[data-testid=history-row-1]").Click();

        Assert.That(opened, Is.EqualTo(Rows[1]));
    }

    [Test]
    public void Loading_ShowsTheSpinnerOnly()
    {
        var cut = Context.Render<HistoryView>(p => p.Add(x => x.Rows, Rows).Add(x => x.Loading, true).Add(x => x.OnBack, () => { }));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=history-loading]").TextContent, Does.Contain("Loading…"));
            Assert.That(cut.FindAll(".rows"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=history-empty]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=history-error]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=back]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Failed_ShowsTheErrorAndTryAgainInsteadOfTheRows()
    {
        var cut = Context.Render<HistoryView>(p => p.Add(x => x.Rows, []).Add(x => x.Failed, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=history-error]").TextContent, Is.EqualTo("Couldn't load your history."));
            Assert.That(cut.Find("[data-testid=history-error]").GetAttribute("role"), Is.EqualTo("alert"));
            Assert.That(cut.Find("[data-testid=history-retry]").TextContent.Trim(), Is.EqualTo("Try again"));
            Assert.That(cut.FindAll("[data-testid=history-empty]"), Is.Empty);
        });
    }

    [Test]
    public void TryAgain_RaisesOnRetry()
    {
        var retries = 0;
        var cut = Context.Render<HistoryView>(p => p
            .Add(x => x.Rows, [])
            .Add(x => x.Failed, true)
            .Add(x => x.OnRetry, () => retries++));

        cut.Find("[data-testid=history-retry]").Click();

        Assert.That(retries, Is.EqualTo(1));
    }

    [Test]
    public void Failed_FocusesTryAgain()
    {
        var cut = Context.Render<HistoryView>(p => p.Add(x => x.Rows, []).Add(x => x.Failed, true));

        cut.WaitForAssertion(() => Focuses().Single().Arguments[0]
            .ShouldBeElementReferenceTo(cut.Find("[data-testid=history-retry]")));
    }

    [Test]
    public void Failed_RenderedAgain_DoesNotFocusAgain()
    {
        var cut = Context.Render<HistoryView>(p => p.Add(x => x.Rows, []).Add(x => x.Failed, true));
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Render(p => p.Add(x => x.Failed, true));

        Assert.That(Focuses(), Has.Count.EqualTo(1));
    }

    [Test]
    public void FailedAfterLoading_FocusesTryAgainAgain()
    {
        var cut = Context.Render<HistoryView>(p => p.Add(x => x.Rows, []).Add(x => x.Failed, true));
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Render(p => p.Add(x => x.Loading, true));
        cut.Render(p => p.Add(x => x.Loading, false));

        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(2)));
    }

    [Test]
    public void FocusFails_LogsAWarningAndKeepsTheError()
    {
        var logger = new FakeLogger<HistoryView>();
        Context.Services.AddSingleton<ILogger<HistoryView>>(logger);
        Context.JSInterop.SetupVoid("Blazor._internal.domWrapper.focus", _ => true).SetException(new JSException("Focus failed"));

        var cut = Context.Render<HistoryView>(p => p.Add(x => x.Rows, []).Add(x => x.Failed, true));

        cut.WaitForAssertion(() => Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.HistoryRetryFocusFailed, LogLevel.Warning, "Focusing Try again failed") })));
        Assert.Multiple(() =>
        {
            Assert.That(logger.LatestRecord.Exception, Is.TypeOf<JSException>());
            Assert.That(cut.FindAll("[data-testid=history-retry]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Back_RaisesOnBack()
    {
        var back = false;
        var cut = Context.Render<HistoryView>(p => p
            .Add(x => x.Rows, Rows)
            .Add(x => x.OnBack, () => back = true));

        cut.Find("[data-testid=back]").Click();

        Assert.That(back, Is.True);
    }

    private IReadOnlyList<JSRuntimeInvocation> Focuses() =>
        Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"];
}
