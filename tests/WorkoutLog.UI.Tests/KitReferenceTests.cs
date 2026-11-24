using System.Text.Json;
using System.Xml.Linq;

namespace WorkoutLog.UI.Tests;

/// <summary>
/// The Shields.UI kit, the Shields.UI.Stories shell and the Shields.UI.Testing design checks come from NuGet or from the kit's source
/// with -p:UseKitSource=true. Directory.Build.props references them in each project that uses them, and is read to check the references.
/// </summary>
public class KitReferenceTests
{
    private const string PackageCondition = "'$(UseKitSource)' != 'true'";
    private const string SourceCondition = "'$(UseKitSource)' == 'true'";

    [TestCase("Shields.UI")]
    [TestCase("Shields.UI.Stories")]
    [TestCase("Shields.UI.Testing")]
    public void KitPackage_IsReferencedUnlessUsingKitSource(string package)
    {
        var references = KitItemGroupsWhen(PackageCondition).Elements("PackageReference");

        Assert.That(references.Select(r => (string?)r.Attribute("Include")), Does.Contain(package));
    }

    [TestCase("Shields.UI", "src/Shields.UI/Shields.UI.csproj")]
    [TestCase("Shields.UI.Stories", "src/Shields.UI.Stories/Shields.UI.Stories.csproj")]
    [TestCase("Shields.UI.Testing", "src/Shields.UI.Testing/Shields.UI.Testing.csproj")]
    public void KitProject_IsReferencedWhenUsingKitSource(string package, string project)
    {
        var references = KitItemGroupsWhen(SourceCondition).Elements("ProjectReference");

        Assert.That(references.Select(r => (string?)r.Attribute("Include")), Does.Contain($"$(KitSourcePath)/{project}"));
        Assert.That(
            references.Count(r => ((string?)r.Attribute("Include"))!.EndsWith($"/{package}.csproj", StringComparison.Ordinal)),
            Is.EqualTo(1));
    }

    // Dependabot rewrites a lock file only where the package is a direct reference, so a transitive entry goes stale.
    [TestCase("src", "WorkoutLog.App", "Shields.UI")]
    [TestCase("src", "WorkoutLog.UI", "Shields.UI")]
    [TestCase("src", "WorkoutLog.UI.Stories", "Shields.UI")]
    [TestCase("src", "WorkoutLog.UI.Stories", "Shields.UI.Stories")]
    [TestCase("src", "WorkoutLog.UI.Stories.Host", "Shields.UI")]
    [TestCase("src", "WorkoutLog.UI.Stories.Host", "Shields.UI.Stories")]
    [TestCase("tests", "WorkoutLog.UI.Tests", "Shields.UI")]
    [TestCase("tests", "WorkoutLog.UI.Tests", "Shields.UI.Stories")]
    [TestCase("tests", "WorkoutLog.UI.Tests", "Shields.UI.Testing")]
    public void KitPackage_IsADirectEntryInEveryLockFileThatUsesIt(string folder, string project, string package)
    {
        var lockFile = JsonDocument.Parse(RepoFiles.Read(folder, project, "packages.lock.json"));
        var entry = lockFile.RootElement.GetProperty("dependencies").EnumerateObject().First().Value.GetProperty(package);

        Assert.That(entry.GetProperty("type").GetString(), Is.EqualTo("Direct"));
    }

    [TestCase("Shields.UI", "src", "WorkoutLog.UI")]
    [TestCase("Shields.UI.Stories", "src", "WorkoutLog.UI.Stories")]
    [TestCase("Shields.UI.Testing", "tests", "WorkoutLog.UI.Tests")]
    public void KitPackageVersion_IsPinnedCentrallyAndMatchesTheLockFile(string package, string folder, string project)
    {
        var props = XDocument.Parse(RepoFiles.Read("Directory.Packages.props"));
        var version = props.Descendants("PackageVersion")
            .Single(p => (string?)p.Attribute("Include") == package)
            .Attribute("Version")?.Value;

        var lockFile = JsonDocument.Parse(RepoFiles.Read(folder, project, "packages.lock.json"));
        var locked = lockFile.RootElement.GetProperty("dependencies").EnumerateObject().First()
            .Value.GetProperty(package).GetProperty("resolved").GetString();

        Assert.That(version, Is.Not.Null.And.Not.Empty);
        Assert.That(locked, Is.EqualTo(version));
    }

    [Test]
    public void KitPackages_ShareOneVersion()
    {
        var props = XDocument.Parse(RepoFiles.Read("Directory.Packages.props"));
        string? Version(string package) => props.Descendants("PackageVersion")
            .Single(p => (string?)p.Attribute("Include") == package).Attribute("Version")?.Value;

        Assert.That(Version("Shields.UI.Stories"), Is.EqualTo(Version("Shields.UI")));
        Assert.That(Version("Shields.UI.Testing"), Is.EqualTo(Version("Shields.UI")));
    }

    [Test]
    public void KitSourceBuild_MovesTheLockFileToObjAndDoesNotEnforceIt()
    {
        var group = BuildPropsGroups().Single(g => (string?)g.Attribute("Condition") == SourceCondition);

        Assert.That(group.Element("RestoreLockedMode")?.Value, Is.EqualTo("false"));
        Assert.That(
            group.Element("NuGetLockFilePath")?.Value,
            Is.EqualTo("$(MSBuildProjectDirectory)/obj/kit-source.packages.lock.json"));
    }

    [Test]
    public void RestoreLockedMode_IsNeverTurnedOffOutsideTheKitSourceGroup()
    {
        var others = BuildPropsGroups()
            .Where(g => (string?)g.Attribute("Condition") != SourceCondition)
            .SelectMany(g => g.Elements("RestoreLockedMode"));

        Assert.That(others.Select(e => e.Value), Is.All.EqualTo("true"));
    }

    [Test]
    public void KitPath_DefaultsToTheSiblingKitFolderAndIsResolvedFromTheRepoRoot()
    {
        var kitPath = BuildPropsGroups().Elements("KitPath").Single();
        var resolved = BuildPropsGroups().Elements("KitSourcePath").Single();

        Assert.That((string?)kitPath.Attribute("Condition"), Is.EqualTo("'$(KitPath)' == ''"));
        Assert.That(kitPath.Value, Is.EqualTo("../app-ui-kit"));
        Assert.That(
            resolved.Value,
            Is.EqualTo("$([System.IO.Path]::GetFullPath($([System.IO.Path]::Combine('$(MSBuildThisFileDirectory)', '$(KitPath)'))))"));
    }

    private static IEnumerable<XElement> BuildPropsGroups() =>
        XDocument.Parse(RepoFiles.Read("Directory.Build.props")).Root!.Elements("PropertyGroup");

    private static IEnumerable<XElement> KitItemGroupsWhen(string condition) =>
        XDocument.Parse(RepoFiles.Read("Directory.Build.props")).Root!
            .Elements("ItemGroup")
            .Where(group => (string?)group.Attribute("Condition") == condition);
}
