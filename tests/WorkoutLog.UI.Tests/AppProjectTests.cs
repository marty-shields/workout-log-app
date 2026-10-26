using System.Xml.Linq;

namespace WorkoutLog.UI.Tests;

/// <summary>
/// Debug builds of the App embed their assemblies, so the E2E tests can install the APK (D143).
/// The App can't be referenced, so its project file is read.
/// </summary>
public class AppProjectTests
{
    [Test]
    public void DebugBuild_EmbedsAssembliesIntoTheApk()
    {
        var project = XDocument.Parse(RepoFiles.Read("src", "WorkoutLog.App", "WorkoutLog.App.csproj"));

        var debug = project.Root!.Elements("PropertyGroup")
            .Where(group => (string?)group.Attribute("Condition") == "'$(Configuration)' == 'Debug'");

        Assert.That(debug.Elements("EmbedAssembliesIntoApk").Select(e => e.Value), Is.EqualTo(new[] { "true" }));
    }
}
