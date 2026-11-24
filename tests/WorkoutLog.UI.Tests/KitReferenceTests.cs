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
