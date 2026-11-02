using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.Conventions;

public partial class SourceConventionTests
{
    private static readonly string[] SourceExtensions = ["*.cs", "*.razor"];

    private static readonly string[] BuildExtensions = ["*.csproj", "*.props", "*.targets"];

    [GeneratedRegex(@"\bDateTime(Offset)?\s*\.\s*(Now|UtcNow|Today)\b|\busing\s+static\s+(System\s*\.\s*)?DateTime(Offset)?\s*;")]
    private static partial Regex WallClock();

    [GeneratedRegex(@"\bTimeProvider\s*\.\s*System\b|\busing\s+static\s+(System\s*\.\s*)?TimeProvider\s*;")]
    private static partial Regex SystemTimeProvider();

    [GeneratedRegex(@"\bSqlite\w*", RegexOptions.IgnoreCase)]
    private static partial Regex SqliteType();

    [GeneratedRegex(@"\b(Execute\w*|Query\w*|CreateCommand|FindWithQuery\w*)\s*(<[^>]*>)?\s*\(")]
    private static partial Regex SqlCall();

    [GeneratedRegex(@"\bglobal\s+using\b[^;]*sqlite", RegexOptions.IgnoreCase)]
    private static partial Regex GlobalSqliteUsing();

    [GeneratedRegex(@"<Using\b[^>]*sqlite", RegexOptions.IgnoreCase)]
    private static partial Regex SqliteUsingItem();

    [GeneratedRegex(@"^[ \t]*//.*$", RegexOptions.Multiline)]
    private static partial Regex CommentLine();

    private static readonly string[] Registrar = ["src", "WorkoutLog.Core", "CoreServiceCollectionExtensions.cs"];

    private static readonly string[] SqliteRegistration = ["using SQLite;", "var connection = new SQLiteAsyncConnection(databasePath);"];

    private static readonly string[] ClockRegistration = [".AddSingleton(TimeProvider.System)"];

    private static string Code(string source) => CommentLine().Replace(source, "");

    private static string WithoutLines(string source, string[] allowed)
    {
        var remaining = allowed.ToList();
        var kept = source.Split('\n').Where(line => !remaining.Remove(line.Trim()));
        return string.Join('\n', kept);
    }

    private static string Scrub(string file, string source, string[] registration) =>
        WithoutLines(Code(source), file == RepoFiles.PathOf(Registrar) ? registration : []);

    private static IEnumerable<string> ClockReads(string code) =>
        WallClock().Matches(code).Concat(SystemTimeProvider().Matches(code)).Select(m => m.Value);

    private static IEnumerable<string> SqlReaches(string code, bool inDataFolder) =>
        inDataFolder
            ? GlobalSqliteUsing().Matches(code).Select(m => m.Value)
            : SqliteType().Matches(code).Concat(SqlCall().Matches(code)).Concat(GlobalSqliteUsing().Matches(code)).Select(m => m.Value);

    private static IEnumerable<TestCaseData> Cases(IEnumerable<string> files) =>
        files.Select(f => new TestCaseData(f).SetArgDisplayNames(Path.GetRelativePath(RepoFiles.Root, f)));

    private static IEnumerable<string> SourceFiles() =>
        SourceExtensions.SelectMany(pattern => RepoFiles.Find(pattern, "src"));

    private static bool IsUnder(string file, params string[] folder) =>
        file.StartsWith(RepoFiles.PathOf(folder) + Path.DirectorySeparatorChar, StringComparison.Ordinal);

    private static bool IsData(string file) => IsUnder(file, "src", "WorkoutLog.Core", "Data");

    private static IEnumerable<TestCaseData> NonStoriesFiles() =>
        Cases(SourceFiles().Where(f => !IsUnder(f, "src", "WorkoutLog.UI.Stories")));

    private static IEnumerable<TestCaseData> AllSourceFiles() => Cases(SourceFiles());

    private static IEnumerable<TestCaseData> BuildFiles() =>
        Cases(BuildExtensions.SelectMany(pattern => RepoFiles.Find(pattern, "src"))
            .Concat(Directory.EnumerateFiles(RepoFiles.Root, "Directory.Build.*")));

    [TestCaseSource(nameof(NonStoriesFiles))]
    public void Source_ReadsTimeThroughTimeProvider(string file) =>
        Assert.That(ClockReads(Scrub(file, File.ReadAllText(file), ClockRegistration)), Is.Empty,
            "Inject TimeProvider and call GetUtcNow() or GetLocalNow(). Only the registration in CoreServiceCollectionExtensions.cs names TimeProvider.System.");

    [TestCaseSource(nameof(AllSourceFiles))]
    public void Source_ReachesSqlOnlyThroughTheDataFolder(string file) =>
        Assert.That(SqlReaches(Scrub(file, File.ReadAllText(file), SqliteRegistration), IsData(file)), Is.Empty,
            "SQL runs only under src/WorkoutLog.Core/Data/. The only other SQLite lines are the using and the connection in CoreServiceCollectionExtensions.cs. Add a repository method there.");

    [TestCaseSource(nameof(BuildFiles))]
    public void Build_DeclaresNoSqliteUsing(string file) =>
        Assert.That(SqliteUsingItem().Matches(File.ReadAllText(file)).Select(m => m.Value), Is.Empty,
            "Drop the <Using> item for a SQLite type: it reaches every file in the project. SQL belongs in src/WorkoutLog.Core/Data/.");

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
    [TestCase("var t = TimeProvider.System.GetUtcNow();")]
    [TestCase("var t = TimeProvider.System.GetLocalNow();")]
    [TestCase("var clock = TimeProvider . System;")]
    [TestCase("using static System.TimeProvider;")]
    public void Clock_CatchesEveryWayToReadIt(string source) =>
        Assert.That(ClockReads(Code(source)), Is.Not.Empty);

    [TestCase("var t = clock.GetUtcNow();")]
    [TestCase("var d = new DateTime(2024, 1, 1);")]
    [TestCase("var d = DateTime.MinValue;")]
    [TestCase("using System;")]
    [TestCase("/// Uses TimeProvider rather than DateTime.Now.")]
    [TestCase("    // not DateTime.UtcNow or DateTimeOffset.Now\nvar t = clock.GetUtcNow();")]
    [TestCase("    // TimeProvider.System is registered once\nvar t = clock.GetUtcNow();")]
    [TestCase("services.AddSingleton(provider);")]
    public void Clock_AllowsTimeProviderAndComments(string source) =>
        Assert.That(ClockReads(Code(source)), Is.Empty);

    [Test]
    public void Clock_AllowsTheRegistrationOnceOnly()
    {
        var registrar = RepoFiles.PathOf(Registrar);
        var once = "services\n    .AddSingleton(TimeProvider.System)\n    .AddX();";
        Assert.That(ClockReads(Scrub(registrar, once, ClockRegistration)), Is.Empty);
        Assert.That(ClockReads(Scrub(registrar, once + "\nvar t = TimeProvider.System.GetUtcNow();", ClockRegistration)), Is.Not.Empty);
        Assert.That(ClockReads(Scrub(RepoFiles.PathOf("src", "WorkoutLog.Core", "Services", "X.cs"), once, ClockRegistration)), Is.Not.Empty);
    }

    private const string RawSqlInRegistrar =
        "internal static Task<int> PurgeAsync(SQLiteAsyncConnection connection, int id) =>\n    connection.ExecuteAsync($\"DELETE FROM WorkoutRow WHERE Id = {id}\");";

    private const string AliasUse = "using var db = new Db(path);\nreturn db.Execute($\"DELETE FROM WorkoutRow WHERE Id = {id}\");";

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
    [TestCase("global using Db = SQLite.SQLiteConnection;")]
    [TestCase("@using Db = SQLite.SQLiteConnection")]
    [TestCase(AliasUse)]
    [TestCase("dynamic db = Get();\ndb.ExecuteAsync($\"DELETE FROM WorkoutRow WHERE Id = {id}\");")]
    [TestCase("db.Execute(\n    $@\"DELETE FROM A WHERE Id = {id}\");")]
    [TestCase("db.Execute(@$\"DELETE FROM A WHERE Id = {id}\");")]
    [TestCase("db.ExecuteScalar<int>($\"\"\"SELECT {x}\"\"\");")]
    [TestCase("db.QueryAsync<A>($\"SELECT * FROM {table}\");")]
    [TestCase("db.Execute(\"DELETE FROM A WHERE Id = ?\", id);")]
    [TestCase("connection.ExecuteAsync(\"PRAGMA journal_mode=WAL\").GetAwaiter().GetResult();")]
    [TestCase(".AddSingleton<Func<int, Task<int>>>(_ => id => connection.ExecuteAsync(\"DELETE FROM WorkoutRow WHERE Id = \" + id))")]
    [TestCase(".AddSingleton<Func<string, Task<int>>>(_ => sql => connection.ExecuteAsync(sql))")]
    [TestCase("command.Execute (x);")]
    public void Sql_CatchesEveryWayToReachTheDatabaseOutsideData(string source) =>
        Assert.That(SqlReaches(Code(source), inDataFolder: false), Is.Not.Empty);

    [TestCase("repository.SaveAsync(workout);")]
    [TestCase("var sql = $\"Hello {name}\";")]
    [TestCase("var s = $\"Hello {name}\";")]
    [TestCase("/// Full path of the SQLite database file.")]
    [TestCase("    // SQLiteAsyncConnection is registered by Core\nvar x = 1;")]
    [TestCase("global using System.Linq;")]
    public void Sql_AllowsRepositoriesAndComments(string source) =>
        Assert.That(SqlReaches(Code(source), inDataFolder: false), Is.Empty);

    [Test]
    public void Sql_RegistrationExemptsOnlyItsTwoLines()
    {
        var registrar = RepoFiles.PathOf(Registrar);
        var registration = "using SQLite;\nvar connection = new SQLiteAsyncConnection(databasePath);";
        Assert.That(SqlReaches(Scrub(registrar, registration, SqliteRegistration), false), Is.Empty);
        Assert.That(SqlReaches(Scrub(registrar, registration + "\n" + RawSqlInRegistrar, SqliteRegistration), false), Is.Not.Empty);
        Assert.That(SqlReaches(Scrub(registrar, registration + "\nvar other = new SQLiteAsyncConnection(path);", SqliteRegistration), false), Is.Not.Empty);
        Assert.That(SqlReaches(Scrub(registrar, registration + "\n" + registration, SqliteRegistration), false), Is.Not.Empty);
        Assert.That(SqlReaches(Scrub(RepoFiles.PathOf("src", "WorkoutLog.Core", "Services", "X.cs"), registration, SqliteRegistration), false), Is.Not.Empty);
    }

    [Test]
    public void Sql_TheRealRegistrarPasses()
    {
        var registrar = RepoFiles.PathOf(Registrar);
        Assert.That(SqlReaches(Scrub(registrar, File.ReadAllText(registrar), SqliteRegistration), false), Is.Empty);
        Assert.That(SqlReaches(Scrub(registrar, File.ReadAllText(registrar) + "\nconnection.ExecuteAsync(\"PRAGMA journal_mode=WAL\");", SqliteRegistration), false), Is.Not.Empty);
    }

    [TestCase("global using Db = SQLite.SQLiteConnection;", true)]
    [TestCase("global using SQLite;", true)]
    [TestCase("using SQLite;\nSQLiteAsyncConnection db;\ndb.Execute($\"x {y}\");", false)]
    public void Sql_InTheDataFolderOnlyAGlobalUsingLeaks(string source, bool leaks) =>
        Assert.That(SqlReaches(Code(source), inDataFolder: true), leaks ? Is.Not.Empty : Is.Empty);

    [TestCase("<ItemGroup>\n  <Using Include=\"SQLite.SQLiteConnection\" Alias=\"Db\" />\n</ItemGroup>")]
    [TestCase("<Using Include=\"SQLite\" />")]
    [TestCase("<Using Alias=\"Db\" Include=\"Microsoft.Data.Sqlite.SqliteConnection\" />")]
    public void BuildUsing_CatchesASqliteUsingItem(string xml) =>
        Assert.That(SqliteUsingItem().IsMatch(xml));

    [TestCase("<Using Include=\"NUnit.Framework\" />")]
    [TestCase("<PackageReference Include=\"sqlite-net-pcl\" />")]
    [TestCase("<Using Include=\"WorkoutLog.Core\" Alias=\"Core\" />")]
    public void BuildUsing_AllowsOtherItems(string xml) =>
        Assert.That(SqliteUsingItem().IsMatch(xml), Is.False);
}
