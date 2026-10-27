using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Reqnroll;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

[Binding]
public sealed partial class SettingsSteps(
    DashboardScreen dashboard, SettingsScreen settings, Journeys journeys, Native native, DeviceClock clock, ScenarioState state)
{
    [When(@"^I tap Settings on the Dashboard$")]
    public void WhenITapSettingsOnTheDashboard()
    {
        journeys.WaitFor(Journeys.Screen.Dashboard);
        dashboard.OpenSettings();
    }

    [Given(@"^I have opened Settings$")]
    public void GivenIHaveOpenedSettings()
    {
        WhenITapSettingsOnTheDashboard();
        journeys.WaitFor(Journeys.Screen.Settings);
    }

    [Then(@"^Settings says ""(.+)""$")]
    public void ThenSettingsSays(string text) => Assert.That(settings.ExportText(), Is.EqualTo(text));

    [Then(@"^Export is disabled$")]
    public void ThenExportIsDisabled() => Assert.That(settings.ExportEnabled(), Is.False);

    [When(@"^I tap Export$")]
    public void WhenITapExport() => settings.Export();

    [Then(@"^the share sheet opens$")]
    public void ThenTheShareSheetOpens() => native.CloseShareSheet();

    [Then(@"^the exported file holds the saved workouts$")]
    public void ThenTheExportedFileHoldsTheSavedWorkouts()
    {
        var name = ExportedFileName();
        var file = JsonNode.Parse(AppiumSetup.Adb.Shell("run-as", AppiumSetup.Package, "cat", $"cache/{name}"))!;

        var exportedAt = DateTimeOffset.ParseExact((string)file["exportedAt"]!, "yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal);
        var local = clock.ToLocal(exportedAt);
        Assert.Multiple(() =>
        {
            Assert.That(name, Is.EqualTo($"workout-log-{local:yyyy-MM-dd-HHmm}.json"));
            Assert.That(clock.UtcNow() - exportedAt, Is.InRange(TimeSpan.Zero, TimeSpan.FromMinutes(5)));
            Assert.That((int)file["formatVersion"]!, Is.EqualTo(1));
            Assert.That((string)file["appVersion"]!, Is.Not.Empty);
        });

        var expected = new JsonArray(state.Seeded.OrderBy(s => s.Value).Select(s => (JsonNode)new JsonObject
        {
            ["startedAt"] = UtcText(s.Value),
            ["finishedAt"] = UtcText(state.SeededFinished[s.Key]),
            ["exercises"] = new JsonArray(state.SeededExercises[s.Key].Select(ExportJson.Exercise).ToArray()),
        }).ToArray());
        var workouts = new JsonArray(file["workouts"]!.AsArray().Select(w => (JsonNode)new JsonObject
        {
            ["startedAt"] = w!["startedAt"]!.DeepClone(),
            ["finishedAt"] = w["finishedAt"]!.DeepClone(),
            ["exercises"] = w["exercises"]!.DeepClone(),
        }).ToArray());
        Assert.That(workouts.ToJsonString(), Is.EqualTo(expected.ToJsonString()));
    }

    /// <summary>A device local time as the file writes it: UTC with 7 fractional digits.</summary>
    private string UtcText(DateTime local) =>
        ExportJson.Time(clock.FromLocal(local));

    /// <summary>The one export file in the app's cache folder.</summary>
    private static string ExportedFileName()
    {
        var files = AppiumSetup.Adb.Shell("run-as", AppiumSetup.Package, "ls", "cache")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(f => ExportFile().IsMatch(f))
            .ToList();
        Assert.That(files, Has.Count.EqualTo(1), "export files in the cache folder");
        return files[0];
    }

    [GeneratedRegex(@"^workout-log-\d{4}-\d{2}-\d{2}-\d{4}\.json$")]
    private static partial Regex ExportFile();
}
