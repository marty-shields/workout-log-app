using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Import;
using WorkoutLog.UI.Logging;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class ImportReviewPageTests : ComponentTestBase
{
    private static readonly ImportReview Review = new("f.json", 4, 2, 1, 1, 0, PhoneCount: 2);
    private static readonly ImportResult Imported = new(ImportStatus.Imported, 2, 1, 0);

    private FakeImportService import = null!;
    private FakeImportFileOpener opener = null!;
    private ImportSession session = null!;
    private FakeLogger<ImportReviewPage> logger = null!;
    private int backs;
    private int imports;

    [SetUp]
    public void SetUp()
    {
        import = new FakeImportService();
        Context.Services.AddSingleton<IImportService>(import);
        opener = new FakeImportFileOpener();
        Context.Services.AddSingleton<IImportFileOpener>(opener);
        session = new ImportSession { Review = Review };
        Context.Services.AddSingleton(session);
        logger = new FakeLogger<ImportReviewPage>();
        Context.Services.AddSingleton<ILogger<ImportReviewPage>>(logger);
        backs = 0;
        imports = 0;
    }

    [Test]
    public void Shows_TheSessionsReview()
    {
        var cut = Render();

        Assert.That(cut.Find("[data-testid=review-file-name]").TextContent, Is.EqualTo("f.json"));
    }

    [Test]
    public void NoReview_GoesBack()
    {
        session.Review = null;

        var cut = Render();

        Assert.Multiple(() =>
        {
            Assert.That(backs, Is.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=review-file-name]"), Is.Empty);
        });
    }

    [Test]
    public void Cancel_ClearsTheSessionAndGoesBack()
    {
        var cut = Render();

        cut.Find("[data-testid=review-cancel]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(backs, Is.EqualTo(1));
            Assert.That(session.Review, Is.Null);
            Assert.That(import.Calls, Is.Empty);
        });
    }

    [Test]
    public async Task AndroidBack_ClearsTheSessionAndGoesBack()
    {
        var cut = Render();

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(backs, Is.EqualTo(1));
            Assert.That(session.Review, Is.Null);
        });
    }

    [Test]
    public async Task Import_AddsWithTheChosenDifferentChoiceThenGoesToSettingsWithTheResult()
    {
        var cut = Render();
        cut.Find("[data-testid=review-use-file]").Change(true);

        cut.Find("[data-testid=review-import]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(import.Calls, Is.EqualTo(new[] { (Review, ImportMode.Add, DifferentChoice.UseFiles) }));
            Assert.That(cut.Find("[data-testid=review-import]").TextContent.Trim(), Is.EqualTo("Importing…"));
        });
        import.Complete(Imported);
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(imports, Is.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(session.Done, Is.EqualTo(Imported));
            Assert.That(session.Review, Is.Null);
            Assert.That(backs, Is.Zero);
        });
    }

    [Test]
    public async Task WhileImporting_TapsCancelAndBackAreIgnored()
    {
        var cut = Render();
        cut.Find("[data-testid=review-import]").Click();

        cut.Find("[data-testid=review-import]").Click();
        cut.Find("[data-testid=back]").Click();
        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(import.Calls, Has.Count.EqualTo(1));
            Assert.That(backs, Is.Zero);
            Assert.That(session.Review, Is.SameAs(Review));
        });
    }

    [Test]
    public async Task ImportFails_ShowsTheFailedMessageAndStays()
    {
        var cut = Render();
        cut.Find("[data-testid=review-import]").Click();

        import.Complete(new ImportResult(ImportStatus.Failed, 0, 0, 0));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=review-failed]").TextContent, Is.EqualTo("Couldn't import your workouts. Try again")));
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=review-import]").TextContent.Trim(), Is.EqualTo("Import"));
            Assert.That(imports, Is.Zero);
            Assert.That(session.Done, Is.Null);
            Assert.That(session.Review, Is.SameAs(Review));
        });
    }

    [Test]
    public async Task ImportAgainAfterAFailure_ClearsTheMessage()
    {
        var cut = Render();
        cut.Find("[data-testid=review-import]").Click();
        import.Complete(new ImportResult(ImportStatus.Failed, 0, 0, 0));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=review-failed]"), Has.Count.EqualTo(1)));

        cut.Find("[data-testid=review-import]").Click();

        Assert.That(cut.FindAll("[data-testid=review-failed]"), Is.Empty);
    }

    [Test]
    public async Task ReplaceAll_AsksFirstThenReplacesOnConfirm()
    {
        var cut = Render();
        cut.Find("[data-testid=review-replace]").Change(true);

        cut.Find("[data-testid=review-import]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=replace-dialog] .title").TextContent, Is.EqualTo("Delete your 2 workouts and import 4?"));
            Assert.That(import.Calls, Is.Empty);
        });
        cut.Find("[data-testid=replace-dialog-confirm]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=replace-dialog]"), Is.Empty);
            Assert.That(import.Calls, Is.EqualTo(new[] { (Review, ImportMode.ReplaceAll, DifferentChoice.KeepPhones) }));
        });
        import.Complete(new ImportResult(ImportStatus.Imported, 4, 0, 0));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(imports, Is.EqualTo(1)));
    }

    [Test]
    public async Task ReplaceAll_ConfirmTwice_ImportsOnce()
    {
        var cut = Render();
        cut.Find("[data-testid=review-replace]").Change(true);
        cut.Find("[data-testid=review-import]").Click();
        var view = cut.FindComponent<ImportReviewView>().Instance;

        var first = cut.InvokeAsync(() => view.OnConfirmReplace.InvokeAsync());
        var second = cut.InvokeAsync(() => view.OnConfirmReplace.InvokeAsync());
        await second;

        Assert.That(import.Calls, Has.Count.EqualTo(1));
        import.Complete(new ImportResult(ImportStatus.Imported, 4, 0, 0));
        await first;
    }

    [Test]
    public void ReplaceAll_CancelClosesTheDialogWithoutImporting()
    {
        var cut = Render();
        cut.Find("[data-testid=review-replace]").Change(true);
        cut.Find("[data-testid=review-import]").Click();

        cut.Find("[data-testid=replace-dialog-cancel]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=replace-dialog]"), Is.Empty);
            Assert.That(import.Calls, Is.Empty);
            Assert.That(backs, Is.Zero);
        });
    }

    [Test]
    public async Task ReplaceAll_AndroidBackClosesTheDialogOnly()
    {
        var cut = Render();
        cut.Find("[data-testid=review-replace]").Change(true);
        cut.Find("[data-testid=review-import]").Click();

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=replace-dialog]"), Is.Empty);
            Assert.That(backs, Is.Zero);
        });
    }

    [Test]
    public async Task ChooseAnother_ShowsReadingThenReviewsTheNewFile()
    {
        session.Review = ImportReview.Rejected("photo.json", ImportError.NotAnExport);
        var next = new ImportReview("next.json", 3, 3, 0, 0, 0, 2);
        var cut = Render();

        cut.Find("[data-testid=review-choose]").Click();

        Assert.That(cut.Find("[data-testid=review-choose]").TextContent.Trim(), Is.EqualTo("Reading…"));
        opener.Complete(next);
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=review-file-name]").TextContent, Is.EqualTo("next.json")));
        Assert.That(session.Review, Is.SameAs(next));
    }

    [Test]
    public async Task ChooseAnother_PickerCancelled_KeepsTheError()
    {
        session.Review = ImportReview.Rejected("photo.json", ImportError.NotAnExport);
        var cut = Render();

        cut.Find("[data-testid=review-choose]").Click();
        opener.Complete(null);
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=review-choose]").TextContent.Trim(), Is.EqualTo("Choose another file")));
        Assert.That(cut.Find("[data-testid=review-error]").TextContent, Is.EqualTo("This file isn't a Workout Log export"));
    }

    [Test]
    public async Task ChooseAnother_PickerThrows_ShowsTheFailedMessageInPlaceOfTheErrorAndLogs()
    {
        session.Review = ImportReview.Rejected("photo.json", ImportError.NotAnExport);
        var exception = new InvalidOperationException("No picker");
        var cut = Render();

        cut.Find("[data-testid=review-choose]").Click();
        opener.Fail(exception);
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=review-error]").TextContent, Is.EqualTo("Couldn't import your workouts. Try again")));
        Assert.Multiple(() =>
        {
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.ReviewPickFailed, LogLevel.Error, "Picking or opening another import file failed") }));
            Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
        });
    }

    [Test]
    public async Task ChooseAnother_ReadFailed_ShowsTheFailedMessage()
    {
        session.Review = ImportReview.Rejected("photo.json", ImportError.NotAnExport);
        var cut = Render();

        cut.Find("[data-testid=review-choose]").Click();
        opener.Complete(ImportReview.Rejected("next.json", ImportError.Failed));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=review-error]").TextContent, Is.EqualTo("Couldn't import your workouts. Try again")));
        Assert.That(cut.Find("[data-testid=review-file-name]").TextContent, Is.EqualTo("photo.json"));
    }

    [Test]
    public async Task ChooseAnother_AnotherBadFile_ShowsItsError()
    {
        session.Review = ImportReview.Rejected("photo.json", ImportError.NotAnExport);
        var cut = Render();

        cut.Find("[data-testid=review-choose]").Click();
        opener.Complete(ImportReview.Rejected("big.json", ImportError.TooLarge));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=review-error]").TextContent, Is.EqualTo("This file is too large to import")));
    }

    [Test]
    public void ChooseAnother_TapsWhileReadingAreIgnoredAndCancelStillWorks()
    {
        session.Review = ImportReview.Rejected("photo.json", ImportError.NotAnExport);
        var cut = Render();

        cut.Find("[data-testid=review-choose]").Click();
        cut.Find("[data-testid=review-choose]").Click();
        cut.Find("[data-testid=review-cancel]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(opener.Calls, Is.EqualTo(1));
            Assert.That(backs, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task ChooseAnother_Disposed_WhileReading_DoesNotChangeTheSession()
    {
        session.Review = ImportReview.Rejected("photo.json", ImportError.NotAnExport);
        var cut = Render();
        cut.Find("[data-testid=review-choose]").Click();
        cut.Find("[data-testid=review-cancel]").Click();

        await Context.DisposeComponentsAsync();
        opener.Complete(new ImportReview("next.json", 3, 3, 0, 0, 0, 2));
        await WaitForRendererAsync();

        Assert.That(session.Review, Is.Null);
    }

    private IRenderedComponent<ImportReviewPage> Render() =>
        Context.Render<ImportReviewPage>(p => p.Add(x => x.OnBack, () => backs++).Add(x => x.OnImported, () => imports++));

    /// <summary>Records each import; each waits until the test completes it.</summary>
    private sealed class FakeImportService : IImportService
    {
        private TaskCompletionSource<ImportResult> pending = new();

        public List<(ImportReview Review, ImportMode Mode, DifferentChoice Different)> Calls { get; } = [];

        public Task<ImportReview> ReadAsync(Stream stream, string fileName) => throw new NotSupportedException();

        public Task<ImportResult> ImportAsync(ImportReview review, ImportMode mode, DifferentChoice different)
        {
            Calls.Add((review, mode, different));
            pending = new TaskCompletionSource<ImportResult>();
            return pending.Task;
        }

        public void Complete(ImportResult result) => pending.SetResult(result);
    }
}
