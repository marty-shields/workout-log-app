using System.Text.Json.Nodes;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Seeding;

/// <summary>
/// Seeds a build that <see cref="WorkoutSeeder"/> can't copy a database into (run-as works only on the debuggable Debug app):
/// puts an export file in Downloads, imports it through Settings, then restarts the app. It takes only workouts Import accepts.
/// </summary>
public sealed class ImportSeeder(WebView web, Native native, Journeys journeys, DashboardScreen dashboard, SettingsScreen settings, ImportReviewScreen review)
{
    private const string FileName = "e2e-seed.json";

    /// <summary>Imports <paramref name="workouts"/>, as the export writes them, into an empty app; their ids are 1…n in the order given.</summary>
    public void Seed(IReadOnlyList<JsonObject> workouts)
    {
        AppiumSetup.Adb.WriteToDownloads(FileName, ExportJson.File(workouts).ToJsonString());
        try
        {
            native.Launch();
            journeys.WaitFor(Journeys.Screen.Dashboard);
            dashboard.OpenSettings();
            journeys.WaitFor(Journeys.Screen.Settings);
            settings.Import();
            native.PickFile(FileName);
            journeys.WaitFor(Journeys.Screen.ImportReview);
            web.Eventually(review.Counts, Is.EqualTo((workouts.Count, 0, 0, 0)), "seed workouts new, already, different and skipped");
            review.Import();
            web.Eventually(settings.ImportMessage, Is.EqualTo($"Added {workouts.Count} workout{(workouts.Count == 1 ? "" : "s")}"), "seed import");
        }
        finally
        {
            AppiumSetup.Adb.RemoveFromDownloads(FileName);
        }

        native.Restart();
    }
}
