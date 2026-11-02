using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.Conventions;

/// <summary>The source reads time through TimeProvider and keeps SQL text under Core's Data folder.</summary>
public partial class SourceConventionTests
{
    private static readonly string[] SourceExtensions = ["*.cs", "*.razor"];

    [GeneratedRegex(@"\b(DateTime|DateTimeOffset)\.(Now|UtcNow)\b")]
    private static partial Regex WallClock();

    [GeneratedRegex("""\b(Execute|Query)\w*(<[^>]*>)?\(\s*(\$+@?|@\$+)"{1,3}""")]
    private static partial Regex InterpolatedSql();

    private static IEnumerable<TestCaseData> Files(Func<string, bool> include) =>
        SourceExtensions
            .SelectMany(pattern => RepoFiles.Find(pattern, "src"))
            .Where(include)
            .Select(f => new TestCaseData(f).SetArgDisplayNames(Path.GetRelativePath(RepoFiles.Root, f)));

    private static bool IsUnder(string file, params string[] folder) =>
        file.StartsWith(RepoFiles.PathOf(folder) + Path.DirectorySeparatorChar, StringComparison.Ordinal);

    private static IEnumerable<TestCaseData> NonStoriesFiles() =>
        Files(f => !IsUnder(f, "src", "WorkoutLog.UI.Stories"));

    private static IEnumerable<TestCaseData> NonDataFiles() =>
        Files(f => !IsUnder(f, "src", "WorkoutLog.Core", "Data"));

    [TestCaseSource(nameof(NonStoriesFiles))]
    public void Source_ReadsTimeThroughTimeProvider(string file) =>
        Assert.That(WallClock().Matches(File.ReadAllText(file)).Select(m => m.Value), Is.Empty,
            "Inject TimeProvider and call GetUtcNow() or GetLocalNow().");

    [TestCaseSource(nameof(NonDataFiles))]
    public void Source_KeepsInterpolatedSqlInTheDataFolder(string file) =>
        Assert.That(InterpolatedSql().Matches(File.ReadAllText(file)).Select(m => m.Value), Is.Empty,
            "Interpolated SQL belongs under src/WorkoutLog.Core/Data/. Elsewhere, pass values as ? parameters.");

    [Test]
    public void WallClockPattern_FindsTheCallsItGuards()
    {
        Assert.That(WallClock().IsMatch("var t = DateTime.Now;"));
        Assert.That(WallClock().IsMatch("var t = DateTime.UtcNow;"));
        Assert.That(WallClock().IsMatch("var t = DateTimeOffset.Now;"));
        Assert.That(WallClock().IsMatch("var t = clock.GetUtcNow();"), Is.False);
    }

    [Test]
    public void InterpolatedSqlPattern_FindsTheCallsItGuards()
    {
        Assert.That(InterpolatedSql().IsMatch("db.Execute($\"DELETE FROM A WHERE Id = {id}\");"));
        Assert.That(InterpolatedSql().IsMatch("db.Query<Workout>($\"SELECT * FROM {table}\");"));
        Assert.That(InterpolatedSql().IsMatch("db.ExecuteScalar<int>($\"\"\"SELECT {x}\"\"\");"));
        Assert.That(InterpolatedSql().IsMatch("db.Execute(\"DELETE FROM A WHERE Id = ?\", id);"), Is.False);
    }
}
