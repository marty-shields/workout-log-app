using WorkoutLog.UI.Components.Actions;
using WorkoutLog.UI.Navigation;
using WorkoutLog.UI.Screens;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Screens;

public class ThisWeekViewTests : ComponentTestBase
{
    private const string Parts = "[data-testid=day-strip], [data-testid=week-tiles], [data-testid=week-workouts], [data-testid=week-records], [data-testid=week-muscles], [data-testid=last-week]";

    [Test]
    public void MidWeek_ShowsTitleSettingsDatesAndStreak()
    {
        var cut = MidWeek();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Workout Log"));
            Assert.That(cut.Find("[data-testid=settings]").GetAttribute("aria-label"), Is.EqualTo("Settings"));
            Assert.That(cut.FindAll("[data-testid=back]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=this-week-dates]").TextContent, Is.EqualTo("16 – 22 Nov"));
            Assert.That(cut.Find("[data-testid=this-week-streak]").TextContent, Is.EqualTo("23-week streak"));
        });
    }

    [Test]
    public void MidWeek_ShowsStripTilesAndCardsInOrder()
    {
        var cut = MidWeek();

        Assert.That(cut.FindAll(Parts).Select(e => e.GetAttribute("data-testid")),
            Is.EqualTo(new[] { "day-strip", "week-tiles", "week-workouts", "week-records", "week-muscles" }));
    }

    [Test]
    public void MidWeek_DatesComeBeforeTheStrip()
    {
        var cut = MidWeek();

        Assert.That(cut.FindAll("[data-testid=this-week-dates], [data-testid=day-strip]").Select(e => e.GetAttribute("data-testid")),
            Is.EqualTo(new[] { "this-week-dates", "day-strip" }));
    }

    [Test]
    public void StreakOfZero_IsHidden()
    {
        var cut = Context.Render<ThisWeekView>(p => p.Add(x => x.Monday, SampleThisWeek.Monday).Add(x => x.Today, SampleThisWeek.Wednesday));

        Assert.That(cut.FindAll("[data-testid=this-week-streak]"), Is.Empty);
    }

    [Test]
    public void WeekAcrossTwoMonths_NamesBoth()
    {
        var cut = Context.Render<ThisWeekView>(p => p.Add(x => x.Monday, new DateOnly(2026, 11, 30)).Add(x => x.Today, new DateOnly(2026, 11, 30)));

        Assert.That(cut.Find("[data-testid=this-week-dates]").TextContent, Is.EqualTo("30 Nov – 6 Dec"));
    }

    [Test]
    public void TabBar_ShowsThisWeekCurrent_AndNotTheOldBar()
    {
        var cut = MidWeek();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[aria-current=page]").GetAttribute("data-testid"), Is.EqualTo("this-week-link"));
            Assert.That(cut.FindAll("[data-testid=bottom-nav]"), Is.Empty);
        });
    }

    [Test]
    public void NewWeek_ShowsZerosThePromptAndLastWeekAndNoOtherCards()
    {
        var cut = Context.Render<ThisWeekView>(p => p
            .Add(x => x.Monday, SampleThisWeek.Monday)
            .Add(x => x.Today, SampleThisWeek.Monday)
            .Add(x => x.LastWeek, SampleThisWeek.LastWeek));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".tile .value").Select(v => v.TextContent), Is.EqualTo(new[] { "0", "0m", "0" }));
            Assert.That(cut.Find("[data-testid=week-workouts-none]").TextContent, Is.EqualTo("Nothing yet this week. Tap + to start one."));
            Assert.That(cut.Find("[data-testid=last-week] .detail").TextContent, Is.EqualTo("4 workouts · 4h 10m · 67 sets"));
            Assert.That(cut.FindAll(Parts).Select(e => e.GetAttribute("data-testid")), Is.EqualTo(new[] { "day-strip", "week-tiles", "week-workouts", "last-week" }));
        });
    }

    [Test]
    public void NoWorkoutsEver_HasNoLastWeekRow()
    {
        var cut = Context.Render<ThisWeekView>(p => p.Add(x => x.Monday, SampleThisWeek.Monday).Add(x => x.Today, SampleThisWeek.Wednesday));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=last-week]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=week-workouts-none]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=week-records], [data-testid=week-muscles]"), Is.Empty);
        });
    }

    [Test]
    public void Loading_ShowsASpinnerInsteadOfTheWeek_AndKeepsTheBars()
    {
        var cut = Context.Render<ThisWeekView>(p => p.Add(x => x.Loading, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=this-week-loading]").TextContent, Does.Contain("Loading"));
            Assert.That(cut.FindAll(Parts), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=settings], [data-testid=tab-bar]"), Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void Failed_ShowsTheErrorAndTryAgain_AndKeepsTheBars()
    {
        var retried = 0;
        var cut = Context.Render<ThisWeekView>(p => p.Add(x => x.Failed, true).Add(x => x.OnRetry, () => retried++));

        cut.Find("[data-testid=this-week-retry]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=this-week-error]").TextContent, Is.EqualTo("Couldn't load This week"));
            Assert.That(cut.FindAll(Parts), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=settings], [data-testid=tab-bar]"), Has.Count.EqualTo(2));
            Assert.That(retried, Is.EqualTo(1));
        });
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public void SettingsGear_RaisesOnSettingsInEveryState(bool loading, bool failed)
    {
        var settings = 0;
        var cut = Context.Render<ThisWeekView>(p => p
            .Add(x => x.Loading, loading)
            .Add(x => x.Failed, failed)
            .Add(x => x.OnSettings, () => settings++));

        cut.Find("[data-testid=settings]").Click();

        Assert.That(settings, Is.EqualTo(1));
    }

    [Test]
    public void Callbacks_AreRaised()
    {
        var navigated = new List<MainTab>();
        var newWorkout = 0;
        var workouts = new List<int>();
        var records = new List<int>();
        var cut = Context.Render<ThisWeekView>(p => p
            .Add(x => x.Monday, SampleThisWeek.Monday)
            .Add(x => x.Today, SampleThisWeek.Wednesday)
            .Add(x => x.Workouts, SampleThisWeek.Workouts)
            .Add(x => x.Records, SampleThisWeek.Records)
            .Add(x => x.Muscles, SampleThisWeek.Muscles)
            .Add(x => x.OnNavigate, navigated.Add)
            .Add(x => x.OnNew, () => newWorkout++)
            .Add(x => x.OnOpenWorkout, workouts.Add)
            .Add(x => x.OnOpenRecord, records.Add));

        cut.Find("[data-testid=trends-link]").Click();
        cut.Find("[data-testid=new-link]").Click();
        cut.Find("[data-testid=week-workouts-row-33]").Click();
        cut.Find("[data-testid=week-records-row-8]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(navigated, Is.EqualTo(new[] { MainTab.Trends }));
            Assert.That(newWorkout, Is.EqualTo(1));
            Assert.That(workouts, Is.EqualTo(new[] { 33 }));
            Assert.That(records, Is.EqualTo(new[] { 8 }));
        });
    }

    [Test]
    public void LastWeekRow_RaisesOnOpenLastWeek()
    {
        var opened = 0;
        var cut = Context.Render<ThisWeekView>(p => p
            .Add(x => x.Monday, SampleThisWeek.Monday)
            .Add(x => x.Today, SampleThisWeek.Monday)
            .Add(x => x.LastWeek, SampleThisWeek.LastWeek)
            .Add(x => x.OnOpenLastWeek, () => opened++));

        cut.Find("[data-testid=last-week-row]").Click();

        Assert.That(opened, Is.EqualTo(1));
    }

    [Test]
    public void Settings_UsesTheGearIcon() =>
        Assert.That(MidWeek().FindComponents<IconButton>().Single().Instance.Icon, Is.EqualTo(IconName.Settings));

    private IRenderedComponent<ThisWeekView> MidWeek() =>
        Context.Render<ThisWeekView>(p => p
            .Add(x => x.Monday, SampleThisWeek.Monday)
            .Add(x => x.Today, SampleThisWeek.Wednesday)
            .Add(x => x.Trained, SampleThisWeek.TrainedDays)
            .Add(x => x.Streak, 23)
            .Add(x => x.ThisWeek, SampleThisWeek.ThisWeek)
            .Add(x => x.LastWeekToDate, SampleThisWeek.LastWeekToDate)
            .Add(x => x.Workouts, SampleThisWeek.Workouts)
            .Add(x => x.Records, SampleThisWeek.Records)
            .Add(x => x.Muscles, SampleThisWeek.Muscles));
}
