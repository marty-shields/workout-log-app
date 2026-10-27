using Reqnroll;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Seeding;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

/// <summary>Files in the device's Download folder, Import on Settings, the file picker and Import Review.</summary>
[Binding]
public sealed class ImportSteps(
    WebView web, Native native, DeviceClock clock, Journeys journeys, SettingsScreen settings, ImportReviewScreen review, ScenarioState state)
{
    /// <summary>A workout in a file without a finish time lasts this long, as a seeded one does.</summary>
    private static readonly TimeSpan DefaultLength = TimeSpan.FromMinutes(45);

    [Given(@"^an export file ""(.+)"" in Downloads with these workouts:$")]
    public void GivenAnExportFile(string name, DataTable table)
    {
        var now = clock.LocalNow();
        var workouts = table.Rows.Select(row =>
        {
            var started = SeedTime.Resolve(row["started"], now);
            return ExportJson.Workout(clock.FromLocal(started), clock.FromLocal(started + DefaultLength), ExerciseValues.ParseWorkout(row["exercises"]));
        });

        Push(name, ExportJson.File(workouts).ToJsonString());
    }

    [Given(@"^a file ""(.+)"" in Downloads holding ""(.*)""$")]
    public void GivenAFile(string name, string content) => Push(name, content);

    [When(@"^I import ""(.+)""$")]
    public void WhenIImport(string name)
    {
        settings.Import();
        native.PickFile(name);
        journeys.WaitFor(Journeys.Screen.ImportReview);
    }

    [When(@"^I tap Import and cancel the file picker$")]
    public void WhenITapImportAndCancel()
    {
        settings.Import();
        native.CancelFilePicker();
    }

    [When(@"^I choose another file ""(.+)""$")]
    public void WhenIChooseAnotherFile(string name)
    {
        review.ChooseAnother();
        native.PickFile(name);
    }

    [Then(@"^the review says ""(.+)""$")]
    public void ThenTheReviewSays(string text) => web.Eventually(review.Total, Is.EqualTo(text), "workouts in the file");

    [Then(@"^the review counts (\d+) new, (\d+) already on this phone, (\d+) different and (\d+) that can't be imported$")]
    public void ThenTheReviewCounts(int fresh, int already, int different, int skipped) =>
        Assert.That(review.Counts(), Is.EqualTo((fresh, already, different, skipped)));

    [Then(@"^the review shows ""(.+)""$")]
    public void ThenTheReviewShows(string text) => Assert.That(review.NothingNew(), Is.EqualTo(text));

    [Then(@"^the review shows the error ""(.+)""$")]
    public void ThenTheReviewShowsTheError(string text) => web.Eventually(review.Error, Is.EqualTo(text), "review error");

    [Then(@"^Import on the review is disabled$")]
    public void ThenImportIsDisabled() => Assert.That(review.ImportEnabled(), Is.False);

    [Then(@"^Import on the review reads ""(.+)""$")]
    public void ThenImportReads(string text) => Assert.That(review.ImportText(), Is.EqualTo(text));

    [When(@"^I choose ""(.+)"" on the review$")]
    public void WhenIChoose(string choice) => review.Choose(choice switch
    {
        "Keep this phone's" => "review-keep",
        "Use the file's" => "review-use-file",
        "Replace all my workouts" => "review-replace",
        _ => throw new ArgumentException($"Unknown choice: {choice}.", nameof(choice)),
    });

    [When(@"^I tap Import on the review$")]
    public void WhenITapImport() => review.Import();

    [Then(@"^the review asks ""(.+)""$")]
    public void ThenTheReviewAsks(string title)
    {
        web.WaitUntil(review.PromptOpen, "the prompt didn't show");
        Assert.That(review.PromptTitle(), Is.EqualTo(title));
    }

    [When(@"^I tap Replace on the review's prompt$")]
    public void WhenITapReplace() => review.Replace();

    [Then(@"^Settings shows ""(.+)"" under Import$")]
    public void ThenSettingsShowsUnderImport(string text) => web.Eventually(settings.ImportMessage, Is.EqualTo(text), "message under Import");

    [Then(@"^Settings has no message under Import$")]
    public void ThenSettingsHasNoMessage()
    {
        settings.ExportText();
        web.Settle();
        Assert.Multiple(() =>
        {
            Assert.That(settings.ImportMessage(), Is.Null, "message under Import");
            Assert.That(settings.ImportText(), Is.EqualTo("Import"));
        });
    }

    /// <summary>Removes the files the scenario put in the device's Download folder.</summary>
    [AfterScenario]
    public void RemoveFiles()
    {
        foreach (var name in state.Downloads)
        {
            AppiumSetup.Adb.RemoveFromDownloads(name);
        }
    }

    /// <summary>Writes <paramref name="content"/> to a temp file and copies it to the device's Download folder as <paramref name="name"/>.</summary>
    private void Push(string name, string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"workoutlog-{Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(path, content);
            AppiumSetup.Adb.PushToDownloads(path, name);
            state.Downloads.Add(name);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
