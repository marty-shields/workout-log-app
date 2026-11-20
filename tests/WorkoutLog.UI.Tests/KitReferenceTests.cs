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
            Is.EqualTo(new[] { "$(KitPath)/src/Shields.UI/Shields.UI.csproj" }));
    }

    [Test]
    public void KitPackageVersion_IsPinnedCentrally()
    {
        var props = XDocument.Parse(RepoFiles.Read("Directory.Packages.props"));

        var version = props.Descendants("PackageVersion")
            .Where(p => (string?)p.Attribute("Include") == "Shields.UI")
            .Select(p => (string?)p.Attribute("Version"));

        Assert.That(version, Is.EqualTo(new[] { "1.0.0-pr.19.9" }));
    }

    private static IEnumerable<XElement> ItemGroupsWhen(string condition) =>
        XDocument.Parse(RepoFiles.Read("src", "WorkoutLog.UI", "WorkoutLog.UI.csproj")).Root!
            .Elements("ItemGroup")
            .Where(group => (string?)group.Attribute("Condition") == condition);
}
