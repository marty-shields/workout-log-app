using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace WorkoutLog.UI.Tests.Conventions;

/// <summary>The backup rules include the database file the App gives <c>AddCore</c>, and nothing else.</summary>
public partial class BackupRulesTests
{
    [GeneratedRegex("""AddCore\(\s*Path\.Combine\(\s*FileSystem\.AppDataDirectory,\s*"([^"]+)"\s*\)""")]
    private static partial Regex DatabaseFileInMauiProgram();

    private static string DatabaseFile()
    {
        var match = DatabaseFileInMauiProgram().Match(RepoFiles.Read("src", "WorkoutLog.App", "MauiProgram.cs"));
        Assert.That(match.Success, "MauiProgram no longer calls AddCore(Path.Combine(FileSystem.AppDataDirectory, \"<file>\")); update this test with it.");
        return match.Groups[1].Value;
    }

    [TestCase("cloud-backup")]
    [TestCase("device-transfer")]
    public void BackupRules_IncludeOnlyTheDatabaseFile(string section)
    {
        var rules = XDocument.Parse(RepoFiles.Read("src", "WorkoutLog.App", "Platforms", "Android", "Resources", "xml", "backup_rules.xml"));

        var includes = rules.Root!.Element(section)!.Elements("include")
            .Select(e => ((string?)e.Attribute("domain"), (string?)e.Attribute("path")));

        Assert.That(includes, Is.EqualTo(new[] { ((string?)"file", (string?)DatabaseFile()) }));
    }
}
