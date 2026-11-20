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
    ThisWeekScreen thisWeek, SettingsScreen settings, Journeys journeys, Native native, DeviceClock clock, ScenarioState state, WebView web)
{
    [When(@"^I tap Settings on This week$")]
    public void WhenITapSettingsOnThisWeek()
    {
        journeys.WaitFor(Journeys.Screen.ThisWeek);
        thisWeek.OpenSettings();
    }

    [Given(@"^I have opened Settings$")]
    public void GivenIHaveOpenedSettings()
    {
        WhenITapSettingsOnThisWeek();
        journeys.WaitFor(Journeys.Screen.Settings);
    }

    [Given(@"^the phone's dark theme is (on|off)$")]
    public void GivenThePhonesDarkThemeIs(string onOrOff)
    {
        var replaced = Native.SetDarkTheme(onOrOff == "on" ? "yes" : "no");
        state.PhoneDarkTheme ??= replaced;
    }

    /// <summary>Puts back the phone's dark theme the scenario changed.</summary>
    [AfterScenario]
    public void RestoreDarkTheme()
    {
        if (state.PhoneDarkTheme is { } night)
        {
            Native.SetDarkTheme(night);
        }
    }

    [When(@"^I choose the (System|Light|Dark) theme$")]
    public void WhenIChooseTheTheme(string theme) => settings.ChooseTheme(theme.ToLowerInvariant());

    [Given(@"^an earlier version saved the (Light|Dark) theme$")]
    public void GivenAnEarlierVersionSavedTheTheme(string theme)
    {
        var marker = Guid.NewGuid().ToString("N");
        settings.StoreThemeAsEarlierVersion(theme.ToLowerInvariant(), marker);

        // The WebView writes localStorage to disk seconds later, and a closed app loses what it hasn't written.
        var app = AppiumSetup.App;
        // grep exits 1 when nothing matches yet; a run-as or su failure exits higher and throws.
        web.WaitUntil(() => AppiumSetup.Adb.AppShell(app, "grep", "-rlF", marker, $"{app.DataFolder}/app_webview", ";", "[", "$?", "-le", "1", "]") != "",
            "the WebView didn't write the seeded theme to disk", TimeSpan.FromSeconds(30));
    }

    [Then(@"^the (Light|Dark) theme is applied$")]
    public void ThenTheThemeIsApplied(string theme)
    {
        var expected = theme.ToLowerInvariant();
        journeys.WaitFor(Journeys.Screen.ThisWeek);
        if (!web.Holds(() => settings.AppliedTheme() == expected, TimeSpan.FromSeconds(10)))
        {
            Assert.That(settings.AppliedTheme(), Is.EqualTo(expected), $"theme applied to the page (stored theme: {settings.StoredTheme() ?? "none"})");
        }
    }

    [Then(@"^the (Light|Dark) theme is selected in Settings$")]
    public void ThenTheThemeIsSelectedInSettings(string theme)
    {
        journeys.WaitFor(Journeys.Screen.Settings);
        web.Eventually(() => settings.ThemeSelected(theme.ToLowerInvariant()), Is.True, $"{theme} theme selected");
    }

    [Then(@"^the status bar icons are light$")]
    public void ThenTheStatusBarIconsAreLight() => web.Eventually(Native.StatusBarIcons, Is.EqualTo("light"), "status bar icons");

    [Then(@"^the app version is listed on Settings$")]
    public void ThenTheAppVersionIsListedOnSettings() => Assert.That(settings.Version(), Is.EqualTo($"Version {AppiumSetup.Adb.VersionName(AppiumSetup.App.Package)}"), "the version Settings shows against the installed package's versionName");

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
        var app = AppiumSetup.App;
        var file = JsonNode.Parse(AppiumSetup.Adb.AppShell(app, "cat", $"{app.DataFolder}/cache/{name}"))!;

        var exportedAt = DateTimeOffset.ParseExact((string)file["exportedAt"]!, "yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal);
        var local = clock.ToLocal(exportedAt);
        Assert.Multiple(() =>
        {
            Assert.That(name, Is.EqualTo($"workout-log-{local:yyyy-MM-dd-HHmm}.json"));
            Assert.That(clock.UtcNow() - exportedAt, Is.InRange(TimeSpan.Zero, TimeSpan.FromMinutes(5)));
            Assert.That((int)file["formatVersion"]!, Is.EqualTo(3));
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
        var app = AppiumSetup.App;
        var files = AppiumSetup.Adb.AppShell(app, "ls", $"{app.DataFolder}/cache")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(f => ExportFile().IsMatch(f))
            .ToList();
        Assert.That(files, Has.Count.EqualTo(1), "export files in the cache folder");
        return files[0];
    }

    [GeneratedRegex(@"^workout-log-\d{4}-\d{2}-\d{2}-\d{4}\.json$")]
    private static partial Regex ExportFile();
}
