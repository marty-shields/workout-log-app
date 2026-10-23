using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Tests.Logging;

public class FileLogProviderTests
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
    public void Log_WritesTheUtcTimeThenTheFormattedEntry()
    {
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 10, 23, 7, 5, 9, 42, TimeSpan.FromHours(1)));
        using var provider = new FileLogProvider(new FileLogWriter(folder, FileLogWriter.DefaultMaxBytes), time);

        provider.CreateLogger("Category").Log(LogLevel.Warning, new EventId(1205), "Clock behind", null, (s, _) => s);

        Assert.That(File.ReadAllLines(Path.Combine(folder, FileLogWriter.FileName)),
            Is.EqualTo(new[] { "2026-10-23T06:05:09.042Z WARN Category[1205] Clock behind" }));
    }

    [Test]
    public void Log_LevelNone_WritesNothing()
    {
        using var provider = new FileLogProvider(new FileLogWriter(folder, FileLogWriter.DefaultMaxBytes), TimeProvider.System);
        var logger = provider.CreateLogger("Category");

        logger.Log(LogLevel.None, new EventId(1), "Ignored", null, (s, _) => s);

        Assert.Multiple(() =>
        {
            Assert.That(logger.IsEnabled(LogLevel.None), Is.False);
            Assert.That(Directory.Exists(folder), Is.False);
        });
    }
}
