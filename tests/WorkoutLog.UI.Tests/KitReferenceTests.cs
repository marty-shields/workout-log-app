using System.Text.Json;
using System.Xml.Linq;

namespace WorkoutLog.UI.Tests;

/// <summary>
/// WorkoutLog.UI takes the Shields.UI kit from NuGet, or from the kit's source with -p:UseKitSource=true.
/// The project file is read to check both references.
/// </summary>
public class KitReferenceTests
{
    private const string PackageCondition = "'$(UseKitSource)' != 'true'";
    private const string SourceCondition = "'$(UseKitSource)' == 'true'";

    [Test]
    public void KitPackage_IsReferencedUnlessUsingKitSource()
    {
        var references = ItemGroupsWhen(PackageCondition).Elements("PackageReference");

        Assert.That(references.Select(r => (string?)r.Attribute("Include")), Is.EqualTo(new[] { "Shields.UI" }));
    }

    [Test]
    public void KitProject_IsReferencedWhenUsingKitSource()
    {
        var references = ItemGroupsWhen(SourceCondition).Elements("ProjectReference");

        Assert.That(
            references.Select(r => (string?)r.Attribute("Include")),
            Is.EqualTo(new[] { "$(KitSourcePath)/src/Shields.UI/Shields.UI.csproj" }));
    }

    [Test]
    public void KitPackageVersion_IsPinnedCentrallyAndMatchesTheLockFile()
    {
        var props = XDocument.Parse(RepoFiles.Read("Directory.Packages.props"));
        var version = props.Descendants("PackageVersion")
            .Single(p => (string?)p.Attribute("Include") == "Shields.UI")
            .Attribute("Version")?.Value;

        var lockFile = JsonDocument.Parse(RepoFiles.Read("src", "WorkoutLog.UI", "packages.lock.json"));
        var locked = lockFile.RootElement.GetProperty("dependencies").EnumerateObject().First()
            .Value.GetProperty("Shields.UI").GetProperty("resolved").GetString();

        Assert.That(version, Is.Not.Null.And.Not.Empty);
        Assert.That(locked, Is.EqualTo(version));
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

    private static IEnumerable<XElement> ItemGroupsWhen(string condition) =>
        XDocument.Parse(RepoFiles.Read("src", "WorkoutLog.UI", "WorkoutLog.UI.csproj")).Root!
            .Elements("ItemGroup")
            .Where(group => (string?)group.Attribute("Condition") == condition);
}
