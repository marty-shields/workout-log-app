using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Haptics;
using WorkoutLog.UI.Import;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Stories;

/// <summary>
/// WorkoutLog.UI.Stories is a Razor class library of stories and sample data; WorkoutLog.UI.Stories.Host is the thin
/// WebAssembly site that serves it.
/// </summary>
public class StoriesProjectsTests
{
    private const string Library = "WorkoutLog.UI.Stories";
    private const string Host = "WorkoutLog.UI.Stories.Host";

    [Test]
    public void Library_IsARazorClassLibrary()
    {
        Assert.That(Sdk(Library), Is.EqualTo("Microsoft.NET.Sdk.Razor"));
    }

    [TestCase("Program.cs")]
    [TestCase("App.razor")]
    [TestCase("wwwroot/index.html")]
    [TestCase("wwwroot/iframe.html")]
    [TestCase("Properties/launchSettings.json")]
    public void Library_HoldsNoHostFile(string file)
    {
        Assert.That(File.Exists(RepoFiles.PathOf("src", Library, file)), Is.False);
    }

    [Test]
    public void Host_IsTheWebAssemblySiteOverTheLibrary()
    {
        var project = Project(Host);

        Assert.That(Sdk(Host), Is.EqualTo("Microsoft.NET.Sdk.BlazorWebAssembly"));
        Assert.That(
            project.Descendants("ProjectReference").Select(r => (string?)r.Attribute("Include")),
            Is.EqualTo(new[] { @"..\WorkoutLog.UI.Stories\WorkoutLog.UI.Stories.csproj" }));
        foreach (var file in new[] { "Program.cs", "App.razor", "wwwroot/index.html", "wwwroot/iframe.html", "Properties/launchSettings.json" })
        {
            Assert.That(File.Exists(RepoFiles.PathOf("src", Host, file)), Is.True, file);
        }
    }

    [Test]
    public void Host_ServesOnPort5056()
    {
        Assert.That(RepoFiles.Read("src", Host, "Properties", "launchSettings.json"), Does.Contain("http://localhost:5056"));
    }

    [Test]
    public void Host_RegistersTheSamplesAndGivesBlazingStoryTheLibrary()
    {
        Assert.That(RepoFiles.Read("src", Host, "Program.cs"), Does.Contain("builder.Services.AddWorkoutLogStorySamples();"));
        Assert.That(
            RepoFiles.Read("src", Host, "App.razor"),
            Does.Contain("Assemblies=\"[typeof(StorySampleServices).Assembly]\""));
    }

    [Test]
    public void Library_TakesTheShellAndNothingFromTheHostIsADirectKitReference()
    {
        var hostPackages = Project(Host).Descendants("PackageReference").Select(r => (string?)r.Attribute("Include"));

        Assert.That(hostPackages, Has.None.StartsWith("Shields."));
    }

    [Test]
    public void AddWorkoutLogStorySamples_RegistersTheSevenSampleServices()
    {
        var services = new ServiceCollection().AddSingleton(TimeProvider.System);

        using var provider = services.AddWorkoutLogStorySamples().BuildServiceProvider();

        Assert.Multiple(() =>
        {
            Assert.That(provider.GetRequiredService<IWorkoutService>(), Is.TypeOf<SampleWorkoutService>());
            Assert.That(provider.GetRequiredService<IExerciseCatalog>(), Is.TypeOf<ExerciseCatalog>());
            Assert.That(provider.GetRequiredService<IImportService>(), Is.TypeOf<SampleImportService>());
            Assert.That(provider.GetRequiredService<ILoggedExercisesService>(), Is.TypeOf<SampleLoggedExercisesService>());
            Assert.That(provider.GetRequiredService<ILastSessionService>(), Is.TypeOf<SampleLastSessionService>());
            Assert.That(provider.GetRequiredService<IImportFilePicker>(), Is.TypeOf<SampleImportFilePicker>());
            Assert.That(provider.GetRequiredService<IHaptics>(), Is.TypeOf<SampleHaptics>());
        });
    }

    private static XElement Project(string name) =>
        XDocument.Parse(RepoFiles.Read("src", name, $"{name}.csproj")).Root!;

    private static string? Sdk(string name) => (string?)Project(name).Attribute("Sdk");
}
