using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Import;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Tests.Import;

public class ImportFileOpenerTests
{
    private static readonly ImportReview Review = new("f.json", 1, 1, 0, 0, 0, 0);

    private FakePicker picker = null!;
    private FakeImportService import = null!;
    private FakeLogger<ImportFileOpener> logger = null!;
    private ImportFileOpener opener = null!;

    [SetUp]
    public void SetUp()
    {
        picker = new FakePicker();
        import = new FakeImportService();
        logger = new FakeLogger<ImportFileOpener>();
        opener = new ImportFileOpener(picker, import, logger);
    }

    [Test]
    public async Task Cancelled_ReturnsNullWithoutReading()
    {
        picker.Next = () => Task.FromResult<PickedFile?>(null);

        Assert.That(await opener.OpenAsync(), Is.Null);
        Assert.That(import.Reads, Is.Empty);
    }

    [TestCase(null)]
    [TestCase(5_242_880L)]
    public async Task Picked_ReadsTheStreamWithTheNameAndDisposesIt(long? size)
    {
        var stream = new TrackingStream();
        picker.Next = () => Task.FromResult<PickedFile?>(new PickedFile("f.json", size, () => Task.FromResult<Stream>(stream)));

        Assert.That(await opener.OpenAsync(), Is.SameAs(Review));
        Assert.Multiple(() =>
        {
            Assert.That(import.Reads, Is.EqualTo(new[] { ((Stream)stream, "f.json") }));
            Assert.That(stream.Disposed, Is.True);
            Assert.That(logger.Collector.Count, Is.Zero);
        });
    }

    [Test]
    public async Task OverTheLimitByItsSize_IsTooLargeWithoutOpening()
    {
        var opened = false;
        picker.Next = () => Task.FromResult<PickedFile?>(new PickedFile("big.json", 5_242_881, () =>
        {
            opened = true;
            return Task.FromResult<Stream>(new MemoryStream());
        }));

        Assert.That(await opener.OpenAsync(), Is.EqualTo(ImportReview.Rejected("big.json", ImportError.TooLarge)));
        Assert.Multiple(() =>
        {
            Assert.That(opened, Is.False);
            Assert.That(import.Reads, Is.Empty);
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
            {
                (UiEvent.ImportFileTooLarge, LogLevel.Warning, "The picked import file is 5242881 bytes, over the limit; not read"),
            }));
        });
    }

    [Test]
    public void PickerThrows_Throws()
    {
        picker.Next = () => throw new InvalidOperationException("No picker");

        Assert.That(opener.OpenAsync, Throws.InvalidOperationException);
    }

    [Test]
    public void OpenThrows_Throws()
    {
        picker.Next = () => Task.FromResult<PickedFile?>(new PickedFile("f.json", 10, () => throw new IOException("Gone")));

        Assert.That(opener.OpenAsync, Throws.TypeOf<IOException>());
    }

    private sealed class FakePicker : IImportFilePicker
    {
        public Func<Task<PickedFile?>> Next { get; set; } = () => Task.FromResult<PickedFile?>(null);

        public Task<PickedFile?> PickAsync() => Next();
    }

    /// <summary>Records each read and returns <see cref="Review"/>.</summary>
    private sealed class FakeImportService : IImportService
    {
        public List<(Stream Stream, string FileName)> Reads { get; } = [];

        public Task<ImportReview> ReadAsync(Stream stream, string fileName)
        {
            Reads.Add((stream, fileName));
            return Task.FromResult(Review);
        }

        public Task<ImportResult> ImportAsync(ImportReview review, ImportMode mode, DifferentChoice different) => throw new NotSupportedException();
    }

    private sealed class TrackingStream : MemoryStream
    {
        public bool Disposed { get; private set; }

        public override ValueTask DisposeAsync()
        {
            Disposed = true;
            return base.DisposeAsync();
        }
    }
}
