using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class HistoryPageTests : ComponentTestBase
{
    private static readonly HistoryResult Failed = new([], Failed: true);

    private FakeHistoryService history = null!;

    [SetUp]
    public void SetUp()
    {
        var clock = new FakeTimeProvider();
        clock.SetLocalTimeZone(TimeZoneInfo.CreateCustomTimeZone("UTC+1", TimeSpan.FromHours(1), "UTC+1", "UTC+1"));
        Context.Services.AddSingleton<TimeProvider>(clock);
        history = new FakeHistoryService();
        Context.Services.AddSingleton<IHistoryService>(history);
    }

    [Test]
    public void WhileLoading_ShowsTheSpinnerOnly()
    {
        var cut = Context.Render<HistoryPage>();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=history-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll(".rows"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=history-empty]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=history-error]"), Is.Empty);
            Assert.That(history.Calls, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Loaded_ShowsTheRowsInGivenOrderInThePhonesTimeZone()
    {
        var cut = Context.Render<HistoryPage>();

        history.Complete(new HistoryResult(
        [
            new WorkoutSummary(2, new DateTimeOffset(2026, 10, 20, 23, 30, 0, TimeSpan.Zero)),
            new WorkoutSummary(1, new DateTimeOffset(2026, 10, 20, 17, 5, 0, TimeSpan.Zero)),
        ], Failed: false));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll(".text").Select(t => t.TextContent),
            Is.EqualTo(new[] { "Wed 21 Oct 2026 · 00:30", "Tue 20 Oct 2026 · 18:05" })));
        Assert.That(cut.FindAll("[data-testid^=history-row-]").Select(r => r.GetAttribute("data-testid")),
            Is.EqualTo(new[] { "history-row-2", "history-row-1" }));
    }

    [Test]
    public async Task TapRow_RaisesOnOpenWithTheWorkoutId()
    {
        var opened = new List<int>();
        var cut = Context.Render<HistoryPage>(p => p.Add(x => x.OnOpen, (int id) => opened.Add(id)));
        history.Complete(new HistoryResult(
            [new WorkoutSummary(2, DateTimeOffset.UnixEpoch.AddDays(1)), new WorkoutSummary(1, DateTimeOffset.UnixEpoch)], Failed: false));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=history-row-1]"), Has.Count.EqualTo(1)));

        cut.Find("[data-testid=history-row-1]").Click();

        Assert.That(opened, Is.EqualTo(new[] { 1 }));
    }

    [Test]
    public async Task NoWorkouts_ShowsNoWorkoutsYet()
    {
        var cut = Context.Render<HistoryPage>();

        history.Complete(new HistoryResult([], Failed: false));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=history-empty]").TextContent, Does.Contain("No workouts yet")));
    }

    [Test]
    public async Task LoadFails_ShowsTheErrorAndFocusesTryAgain()
    {
        var cut = Context.Render<HistoryPage>();

        history.Complete(Failed);
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Focuses().Single().Arguments[0]
            .ShouldBeElementReferenceTo(cut.Find("[data-testid=history-retry]")));
        Assert.That(cut.Find("[data-testid=history-error]").TextContent, Is.EqualTo("Couldn't load your history"));
    }

    [Test]
    public async Task TryAgain_ShowsTheSpinnerWhileReloadingThenTheRows()
    {
        var cut = Context.Render<HistoryPage>();
        history.Complete(Failed);
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=history-retry]"), Has.Count.EqualTo(1)));

        cut.Find("[data-testid=history-retry]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(history.Calls, Is.EqualTo(2));
            Assert.That(cut.FindAll("[data-testid=history-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=history-error]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=history-empty]"), Is.Empty);
        });

        history.Complete(new HistoryResult([new WorkoutSummary(1, DateTimeOffset.UnixEpoch)], Failed: false));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=history-row-1]"), Has.Count.EqualTo(1)));
        Assert.That(cut.FindAll("[data-testid=history-error]"), Is.Empty);
        Assert.That(cut.FindAll("[data-testid=history-loading]"), Is.Empty);
    }

    [Test]
    public async Task TryAgain_FailsAgain_ShowsTheErrorAndFocusesTryAgainAgain()
    {
        var cut = Context.Render<HistoryPage>();
        history.Complete(Failed);
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Find("[data-testid=history-retry]").Click();
        history.Complete(Failed);
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(2)));
        Focuses()[^1].Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=history-retry]"));
    }

    [Test]
    public void Back_RaisesOnBack()
    {
        var backs = 0;
        var cut = Context.Render<HistoryPage>(p => p.Add(x => x.OnBack, () => backs++));

        cut.Find("[data-testid=back]").Click();

        Assert.That(backs, Is.EqualTo(1));
    }

    [Test]
    public async Task AndroidBack_RaisesOnBack()
    {
        var backs = 0;
        var cut = Context.Render<HistoryPage>(p => p.Add(x => x.OnBack, () => backs++));

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.That(backs, Is.EqualTo(1));
    }

    private IReadOnlyList<JSRuntimeInvocation> Focuses() =>
        Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"];

    /// <summary>Each call waits until the test completes it.</summary>
    private sealed class FakeHistoryService : IHistoryService
    {
        private TaskCompletionSource<HistoryResult> pending = new();

        public int Calls { get; private set; }

        public Task<HistoryResult> GetAllAsync()
        {
            Calls++;
            pending = new TaskCompletionSource<HistoryResult>();
            return pending.Task;
        }

        /// <summary>Completes the latest call.</summary>
        public void Complete(HistoryResult result) => pending.SetResult(result);
    }
}
