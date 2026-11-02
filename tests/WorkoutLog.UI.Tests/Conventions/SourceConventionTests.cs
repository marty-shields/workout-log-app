using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.Conventions;

/// <summary>The source reads time through TimeProvider and reaches SQLite only from Core's Data folder and its registration.</summary>
public partial class SourceConventionTests
{
    private static readonly string[] SourceExtensions = ["*.cs", "*.razor"];

    [GeneratedRegex(@"\bDateTime(Offset)?\s*\.\s*(Now|UtcNow|Today)\b|\busing\s+static\s+(System\s*\.\s*)?DateTime(Offset)?\s*;")]
    private static partial Regex WallClock();

    [GeneratedRegex(@"\bSqlite\w*", RegexOptions.IgnoreCase)]
    private static partial Regex SqliteApi();

    [GeneratedRegex(@"^[ \t]*//.*$", RegexOptions.Multiline)]
    private static partial Regex CommentLine();

    private static string Code(string source) => CommentLine().Replace(source, "");

    private static readonly string[] SqliteRegistrar = ["src", "WorkoutLog.Core", "CoreServiceCollectionExtensions.cs"];

    private static IEnumerable<TestCaseData> Files(Func<string, bool> include) =>
        SourceExtensions
            .SelectMany(pattern => RepoFiles.Find(pattern, "src"))
            .Where(include)
            .Select(f => new TestCaseData(f).SetArgDisplayNames(Path.GetRelativePath(RepoFiles.Root, f)));

    private static bool IsUnder(string file, params string[] folder) =>
        file.StartsWith(RepoFiles.PathOf(folder) + Path.DirectorySeparatorChar, StringComparison.Ordinal);

    private static IEnumerable<TestCaseData> NonStoriesFiles() =>
        Files(f => !IsUnder(f, "src", "WorkoutLog.UI.Stories"));

    private static IEnumerable<TestCaseData> NonSqliteFiles() =>
        Files(f => !IsUnder(f, "src", "WorkoutLog.Core", "Data") && f != RepoFiles.PathOf(SqliteRegistrar));

    [TestCaseSource(nameof(NonStoriesFiles))]
    public void Source_ReadsTimeThroughTimeProvider(string file) =>
        Assert.That(WallClock().Matches(Code(File.ReadAllText(file))).Select(m => m.Value), Is.Empty,
            "Inject TimeProvider and call GetUtcNow() or GetLocalNow().");

    [TestCaseSource(nameof(NonSqliteFiles))]
    public void Source_UsesSqliteOnlyInTheDataFolder(string file) =>
        Assert.That(SqliteApi().Matches(Code(File.ReadAllText(file))).Select(m => m.Value), Is.Empty,
            "SQL runs only under src/WorkoutLog.Core/Data/, plus the registration in CoreServiceCollectionExtensions.cs. Add a repository method there.");

    [TestCase("var t = DateTime.Now;")]
    [TestCase("var t = DateTime.UtcNow;")]
    [TestCase("var t = DateTime.Today;")]
    [TestCase("var t = DateTimeOffset.Now;")]
    [TestCase("var t = DateTimeOffset.UtcNow;")]
    [TestCase("var t = System.DateTime.Now;")]
    [TestCase("var t = DateTime .Now;")]
    [TestCase("var t = DateTime\n    .Now;")]
    [TestCase("var t = DateTime. \r\n UtcNow;")]
    [TestCase("using static System.DateTime;")]
    [TestCase("using static System.DateTimeOffset;")]
    [TestCase("using   static  System . DateTime ;")]
    [TestCase("// old\nvar t = DateTime.Now;")]
    public void WallClock_CatchesEveryWayToReadIt(string source) =>
        Assert.That(WallClock().IsMatch(Code(source)));

    [TestCase("var t = clock.GetUtcNow();")]
    [TestCase("var d = new DateTime(2024, 1, 1);")]
    [TestCase("var d = DateTime.MinValue;")]
    [TestCase("using System;")]
    [TestCase("/// Uses TimeProvider rather than DateTime.Now.")]
    [TestCase("    // not DateTime.UtcNow or DateTimeOffset.Now\nvar t = clock.GetUtcNow();")]
    public void WallClock_AllowsTimeProviderAndComments(string source) =>
        Assert.That(WallClock().IsMatch(Code(source)), Is.False);

    [TestCase("SQLiteAsyncConnection db = Open();\nvar sql = $\"DELETE FROM A WHERE Id = {id}\"; db.Execute(sql);")]
    [TestCase("SQLiteAsyncConnection db = Open();\ndb.Execute($\"DELETE FROM A WHERE Id = {id}\");")]
    [TestCase("SQLiteAsyncConnection db = Open();\ndb.Execute(string.Format(\"DELETE FROM A WHERE Id = {0}\", id));")]
    [TestCase("SQLiteAsyncConnection db = Open();\ndb.Execute(\"DELETE FROM A WHERE Id = \" + id);")]
    [TestCase("SQLiteAsyncConnection db = Open();\ndb.Execute(string.Concat(\"DELETE FROM A WHERE Id = \", id));")]
    [TestCase("SQLiteAsyncConnection db = Open();\ndb.CreateCommand($\"DELETE FROM A WHERE Id = {id}\");")]
    [TestCase("SQLiteAsyncConnection db = Open();\ndb.FindWithQuery<A>($\"SELECT * FROM A WHERE Id = {id}\");")]
    [TestCase("SQLiteAsyncConnection db = Open();\ndb.Execute(\"DELETE FROM A WHERE Id = ?\", $\"{id}\");")]
    [TestCase("private readonly SQLiteAsyncConnection db;")]
    [TestCase("var db = new SQLiteConnection(path);")]
    [TestCase("using SQLite;")]
    [TestCase("using Microsoft.Data.Sqlite;")]
    public void SqliteApi_CatchesEveryWayToReachTheDatabase(string source) =>
        Assert.That(SqliteApi().IsMatch(Code(source)));

    [TestCase("repository.SaveAsync(workout);")]
    [TestCase("var sql = $\"Hello {name}\";")]
    [TestCase("/// Full path of the SQLite database file.")]
    [TestCase("    // SQLiteAsyncConnection is registered by Core\nvar x = 1;")]
    public void SqliteApi_AllowsRepositoriesAndComments(string source) =>
        Assert.That(SqliteApi().IsMatch(Code(source)), Is.False);
}
