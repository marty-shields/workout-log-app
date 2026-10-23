using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Tests.Logging;

public class FileLogWriterTests
{
    private string folder = "";

    [SetUp]
    public void SetUp() => folder = Path.Combine(Path.GetTempPath(), $"workoutlog-logs-{Guid.NewGuid():N}");

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Test]
    public void Write_NoFolder_CreatesItAndAppendsLines()
    {
        var writer = new FileLogWriter(folder, FileLogWriter.DefaultMaxBytes);

        writer.Write("first");
        writer.Write("second");

        Assert.That(File.ReadAllLines(LogPath()), Is.EqualTo(new[] { "first", "second" }));
    }

    [Test]
    public void Write_FileAtTheCap_MovesItToTheOldFileAndStartsANewOne()
    {
        var writer = new FileLogWriter(folder, 10);
        writer.Write("0123456789");

        writer.Write("next");

        Assert.Multiple(() =>
        {
            Assert.That(File.ReadAllLines(OldPath()), Is.EqualTo(new[] { "0123456789" }));
            Assert.That(File.ReadAllLines(LogPath()), Is.EqualTo(new[] { "next" }));
        });
    }

    [Test]
    public void Write_SecondRollover_ReplacesTheOldFile()
    {
        var writer = new FileLogWriter(folder, 10);
        writer.Write("first line");
        writer.Write("second line");

        writer.Write("third");

        Assert.Multiple(() =>
        {
            Assert.That(File.ReadAllLines(OldPath()), Is.EqualTo(new[] { "second line" }));
            Assert.That(File.ReadAllLines(LogPath()), Is.EqualTo(new[] { "third" }));
            Assert.That(Directory.GetFiles(folder), Has.Length.EqualTo(2));
        });
    }

    [Test]
    public void Write_FolderCantBeCreated_DoesNotThrow()
    {
        File.WriteAllText(folder, "a file where the folder should be");
        try
        {
            Assert.That(() => new FileLogWriter(folder, FileLogWriter.DefaultMaxBytes).Write("line"), Throws.Nothing);
        }
        finally
        {
            File.Delete(folder);
        }
    }

    [Test]
    public void Write_FromManyThreads_KeepsEveryLine()
    {
        var writer = new FileLogWriter(folder, FileLogWriter.DefaultMaxBytes);

        Parallel.For(0, 200, i => writer.Write($"line {i}"));

        Assert.That(File.ReadAllLines(LogPath()), Is.EquivalentTo(Enumerable.Range(0, 200).Select(i => $"line {i}")));
    }

    private string LogPath() => Path.Combine(folder, FileLogWriter.FileName);

    private string OldPath() => Path.Combine(folder, FileLogWriter.OldFileName);
}
