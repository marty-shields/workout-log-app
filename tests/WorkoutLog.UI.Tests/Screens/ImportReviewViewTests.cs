using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.JSInterop;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Logging;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class ImportReviewViewTests : ComponentTestBase
{
    private const string FileName = "workout-log-2026-10-26-1805.json";

    [Test]
    public void Header_ShowsTheTitleFileNameAndTotal()
    {
        var cut = Render(Review(3, 38, 2, 1));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Import workouts"));
            Assert.That(cut.Find("[data-testid=review-file-name]").TextContent, Is.EqualTo(FileName));
            Assert.That(cut.Find("[data-testid=review-total]").TextContent, Is.EqualTo("44 workouts in the file"));
        });
    }

    [TestCase(1, "1 workout in the file")]
    [TestCase(2, "2 workouts in the file")]
    [TestCase(1042, "1,042 workouts in the file")]
    public void Total_IsWorded(int total, string text) =>
        Assert.That(Render(Review(fresh: total)).Find("[data-testid=review-total]").TextContent, Is.EqualTo(text));

    [Test]
    public void Counts_ShowEachGroup()
    {
        var cut = Render(Review(3, 38, 2, 1));

        Assert.Multiple(() =>
        {
            Assert.That(Tile(cut, "review-new"), Is.EqualTo(("3", "New")));
            Assert.That(Tile(cut, "review-already"), Is.EqualTo(("38", "Already on this phone")));
            Assert.That(Tile(cut, "review-different"), Is.EqualTo(("2", "Different on this phone")));
            Assert.That(Tile(cut, "review-skipped"), Is.EqualTo(("1", "Can't be imported")));
        });
    }

    [Test]
    public void HowToImport_OffersAddAndReplaceAllWithTheMode()
    {
        var cut = Render(Review(fresh: 3), mode: ImportMode.ReplaceAll);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=review-mode] legend").TextContent, Is.EqualTo("How to import"));
            Assert.That(cut.FindAll("[data-testid=review-mode] .text").Select(t => t.TextContent),
                Is.EqualTo(new[] { "Add to my workouts", "Replace all my workouts" }));
            Assert.That(cut.Find("[data-testid=review-replace]").HasAttribute("checked"), Is.True);
        });
    }

    [TestCase(41, "Deletes the 41 on this phone")]
    [TestCase(1, "Deletes the 1 on this phone")]
    [TestCase(1042, "Deletes the 1,042 on this phone")]
    [TestCase(0, "No workouts on this phone to delete")]
    public void ReplaceHint_SaysHowManyAreDeleted(int phone, string hint) =>
        Assert.That(Render(Review(fresh: 3) with { PhoneCount = phone }).Find("[data-testid=review-mode] .hint").TextContent, Is.EqualTo(hint));

    [Test]
    public void ChangingTheMode_RaisesModeChanged()
    {
        var modes = new List<ImportMode>();
        var cut = Context.Render<ImportReviewView>(p => p.Add(x => x.Review, Review(fresh: 3)).Add(x => x.ModeChanged, (ImportMode m) => modes.Add(m)));

        cut.Find("[data-testid=review-replace]").Change(true);

        Assert.That(modes, Is.EqualTo(new[] { ImportMode.ReplaceAll }));
    }

    [Test]
    public void DifferentWorkouts_ShownForAddWithDifferentOnes()
    {
        var cut = Render(Review(different: 2), different: DifferentChoice.UseFiles);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=review-different-choice] legend").TextContent, Is.EqualTo("Different workouts"));
            Assert.That(cut.FindAll("[data-testid=review-different-choice] .text").Select(t => t.TextContent),
                Is.EqualTo(new[] { "Keep this phone's", "Use the file's" }));
            Assert.That(cut.Find("[data-testid=review-use-file]").HasAttribute("checked"), Is.True);
        });
    }

    [Test]
    public void DifferentWorkouts_HiddenWithNoneOrReplaceAll()
    {
        Assert.That(Render(Review(fresh: 2)).FindAll("[data-testid=review-different-choice]"), Is.Empty);
        Assert.That(Render(Review(different: 2), mode: ImportMode.ReplaceAll).FindAll("[data-testid=review-different-choice]"), Is.Empty);
    }

    [Test]
    public void ChangingTheDifferentChoice_RaisesDifferentChanged()
    {
        var choices = new List<DifferentChoice>();
        var cut = Context.Render<ImportReviewView>(p => p
            .Add(x => x.Review, Review(different: 2)).Add(x => x.DifferentChanged, (DifferentChoice c) => choices.Add(c)));

        cut.Find("[data-testid=review-use-file]").Change(true);

        Assert.That(choices, Is.EqualTo(new[] { DifferentChoice.UseFiles }));
    }

    [TestCase(1, "1 workout can't be imported and will be skipped")]
    [TestCase(4, "4 workouts can't be imported and will be skipped")]
    [TestCase(1500, "1,500 workouts can't be imported and will be skipped")]
    public void Skipped_IsWorded(int skipped, string text) =>
        Assert.That(Render(Review(fresh: 1, skipped: skipped)).Find("[data-testid=review-skipped-text]").TextContent, Is.EqualTo(text));

    [Test]
    public void NoSkipped_HasNoSkippedText() =>
        Assert.That(Render(Review(fresh: 1)).FindAll("[data-testid=review-skipped-text]"), Is.Empty);

    [Test]
    public void Add_ShowsAnEnabledPrimaryImport()
    {
        var import = Render(Review(fresh: 1)).Find("[data-testid=review-import]");

        Assert.Multiple(() =>
        {
            Assert.That(import.TextContent.Trim(), Is.EqualTo("Import"));
            Assert.That(import.ClassList, Does.Contain("primary"));
            Assert.That(import.HasAttribute("disabled"), Is.False);
        });
    }

    [Test]
    public void ReplaceAll_ShowsADangerReplaceAll()
    {
        var import = Render(Review(already: 3), mode: ImportMode.ReplaceAll).Find("[data-testid=review-import]");

        Assert.Multiple(() =>
        {
            Assert.That(import.TextContent.Trim(), Is.EqualTo("Replace all"));
            Assert.That(import.ClassList, Does.Contain("danger"));
            Assert.That(import.HasAttribute("disabled"), Is.False);
        });
    }

    [TestCase(0, 3, 0, DifferentChoice.KeepPhones)]
    [TestCase(0, 0, 2, DifferentChoice.KeepPhones)]
    [TestCase(0, 0, 0, DifferentChoice.UseFiles)]
    public void NothingNew_SaysSoAndDisablesImport(int already, int different, int skipped, DifferentChoice choice)
    {
        var cut = Render(Review(already: already, different: different, skipped: skipped + (already + different == 0 ? 1 : 0)), different: choice);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=review-nothing-new]").TextContent, Is.EqualTo("Nothing new to import"));
            Assert.That(cut.Find("[data-testid=review-import]").HasAttribute("disabled"), Is.True);
        });
    }

    [Test]
    public void DifferentUsingTheFiles_IsSomethingToImport()
    {
        var cut = Render(Review(already: 3, different: 2), different: DifferentChoice.UseFiles);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=review-nothing-new]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=review-import]").HasAttribute("disabled"), Is.False);
        });
    }

    [Test]
    public void NothingNew_ReplaceAllStillWorks()
    {
        var cut = Render(Review(already: 3), mode: ImportMode.ReplaceAll);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=review-nothing-new]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=review-import]").HasAttribute("disabled"), Is.False);
        });
    }

    [Test]
    public void AllCantBeImported_DisablesImportAndReplaceAll()
    {
        Assert.That(Render(Review(skipped: 3)).Find("[data-testid=review-import]").HasAttribute("disabled"), Is.True);
        Assert.That(Render(Review(skipped: 3), mode: ImportMode.ReplaceAll).Find("[data-testid=review-import]").HasAttribute("disabled"), Is.True);
    }

    [Test]
    public void Import_RaisesOnImport()
    {
        var imports = 0;
        var cut = Context.Render<ImportReviewView>(p => p.Add(x => x.Review, Review(fresh: 1)).Add(x => x.OnImport, () => imports++));

        cut.Find("[data-testid=review-import]").Click();

        Assert.That(imports, Is.EqualTo(1));
    }

    [Test]
    public void Cancel_RaisesOnCancel()
    {
        var cancels = 0;
        var cut = Context.Render<ImportReviewView>(p => p.Add(x => x.Review, Review(fresh: 1)).Add(x => x.OnCancel, () => cancels++));

        cut.Find("[data-testid=review-cancel]").Click();

        Assert.That(cancels, Is.EqualTo(1));
    }

    [Test]
    public void Back_RaisesOnBack()
    {
        var backs = 0;
        var cut = Context.Render<ImportReviewView>(p => p.Add(x => x.Review, Review(fresh: 1)).Add(x => x.OnBack, () => backs++));

        cut.Find("[data-testid=back]").Click();

        Assert.That(backs, Is.EqualTo(1));
    }

    [Test]
    public void Confirming_AsksToReplaceWithTheCounts()
    {
        var confirms = 0;
        var cancels = 0;
        var cut = Context.Render<ImportReviewView>(p => p
            .Add(x => x.Review, Review(3, 38, 2, 1))
            .Add(x => x.Mode, ImportMode.ReplaceAll)
            .Add(x => x.Confirming, true)
            .Add(x => x.OnConfirmReplace, () => confirms++)
            .Add(x => x.OnCancelReplace, () => cancels++));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=replace-dialog] .title").TextContent, Is.EqualTo("Delete your 41 workouts and import 43?"));
            Assert.That(cut.Find("[data-testid=replace-dialog-confirm]").TextContent.Trim(), Is.EqualTo("Replace"));
            Assert.That(cut.Find("[data-testid=replace-dialog-confirm]").ClassList, Does.Contain("danger"));
        });
        cut.Find("[data-testid=replace-dialog-confirm]").Click();
        cut.Find("[data-testid=replace-dialog-cancel]").Click();
        Assert.That((confirms, cancels), Is.EqualTo((1, 1)));
    }

    [TestCase(0, 1, "Delete your 0 workouts and import 1?")]
    [TestCase(1, 1, "Delete your 1 workout and import 1?")]
    [TestCase(1042, 2000, "Delete your 1,042 workouts and import 2,000?")]
    public void ConfirmTitle_IsWorded(int phone, int importable, string title)
    {
        var cut = Context.Render<ImportReviewView>(p => p
            .Add(x => x.Review, Review(fresh: importable) with { PhoneCount = phone })
            .Add(x => x.Mode, ImportMode.ReplaceAll)
            .Add(x => x.Confirming, true));

        Assert.That(cut.Find("[data-testid=replace-dialog] .title").TextContent, Is.EqualTo(title));
    }

    [Test]
    public void NotConfirming_HasNoDialog() =>
        Assert.That(Render(Review(fresh: 1), mode: ImportMode.ReplaceAll).FindAll("[data-testid=replace-dialog]"), Is.Empty);

    [Test]
    public void Importing_ShowsImportBusyAndDisablesCancelAndTheChoices()
    {
        var imports = 0;
        var cut = Context.Render<ImportReviewView>(p => p
            .Add(x => x.Review, Review(fresh: 1, different: 1)).Add(x => x.Importing, true).Add(x => x.OnImport, () => imports++));

        var import = cut.Find("[data-testid=review-import]");
        import.Click();

        Assert.Multiple(() =>
        {
            Assert.That(import.TextContent.Trim(), Is.EqualTo("Importing…"));
            Assert.That(import.GetAttribute("aria-busy"), Is.EqualTo("true"));
            Assert.That(imports, Is.Zero);
            Assert.That(cut.Find("[data-testid=review-cancel]").HasAttribute("disabled"), Is.True);
            Assert.That(cut.Find("[data-testid=review-mode]").HasAttribute("disabled"), Is.True);
            Assert.That(cut.Find("[data-testid=review-different-choice]").HasAttribute("disabled"), Is.True);
        });
    }

    [Test]
    public void ImportFailed_ShowsTheMessageAndFocusesImport()
    {
        var cut = Render(Review(fresh: 1));
        var import = cut.Find("[data-testid=review-import]").GetAttribute("blazor:elementReference");

        cut.Render(p => p.Add(x => x.Failed, true));

        var failed = cut.Find("[data-testid=review-failed]");
        Assert.Multiple(() =>
        {
            Assert.That(failed.TextContent, Is.EqualTo("Couldn't import your workouts. Try again"));
            Assert.That(failed.GetAttribute("role"), Is.EqualTo("alert"));
        });
        cut.WaitForAssertion(() => Assert.That(((ElementReference)Focuses().Single().Arguments[0]!).Id, Is.EqualTo(import)));
    }

    [Test]
    public void ImportFailed_RenderedAgain_DoesNotFocusAgain()
    {
        var cut = Render(Review(fresh: 1));
        cut.Render(p => p.Add(x => x.Failed, true));
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Render(p => p.Add(x => x.Mode, ImportMode.Add));

        Assert.That(Focuses(), Has.Count.EqualTo(1));
    }

    [Test]
    public void ImportFocusFails_LogsAWarningAndKeepsTheMessage()
    {
        var logger = new FakeLogger<ImportReviewView>();
        Context.Services.AddSingleton<ILogger<ImportReviewView>>(logger);
        Context.JSInterop.SetupVoid("Blazor._internal.domWrapper.focus", _ => true).SetException(new JSException("Focus failed"));

        var cut = Render(Review(fresh: 1));
        cut.Render(p => p.Add(x => x.Failed, true));

        cut.WaitForAssertion(() => Assert.That(LoggedEvents.Of(logger),
            Is.EqualTo(new[] { (UiEvent.ReviewImportFocusFailed, LogLevel.Warning, "Focusing Import on the review failed") })));
        Assert.That(cut.FindAll("[data-testid=review-failed]"), Has.Count.EqualTo(1));
    }

    [TestCase(ImportError.NotAnExport, "This file isn't a Workout Log export")]
    [TestCase(ImportError.NewerVersion, "This file is from a newer version of Workout Log. Update the app to import it")]
    [TestCase(ImportError.NoWorkouts, "This file has no workouts")]
    [TestCase(ImportError.TooLarge, "This file is too large to import")]
    [TestCase(ImportError.Failed, "Couldn't import your workouts. Try again")]
    public void FileError_ShowsTheErrorInsteadOfTheCountsAndChoices(ImportError error, string text)
    {
        var cut = Render(ImportReview.Rejected("photo.json", error));

        var shown = cut.Find("[data-testid=review-error]");
        Assert.Multiple(() =>
        {
            Assert.That(shown.TextContent, Is.EqualTo(text));
            Assert.That(shown.GetAttribute("role"), Is.EqualTo("alert"));
            Assert.That(cut.Find("[data-testid=review-file-name]").TextContent, Is.EqualTo("photo.json"));
            Assert.That(cut.FindAll("[data-testid=review-total]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=review-new]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=review-mode]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=review-import]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=review-choose]").TextContent.Trim(), Is.EqualTo("Choose another file"));
            Assert.That(cut.Find("[data-testid=review-cancel]").HasAttribute("disabled"), Is.False);
        });
    }

    [Test]
    public void FileErrorFailed_ShowsTheFailedMessageInPlaceOfTheError()
    {
        var cut = Render(ImportReview.Rejected("photo.json", ImportError.NotAnExport), failed: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=review-error]").TextContent, Is.EqualTo("Couldn't import your workouts. Try again"));
            Assert.That(cut.FindAll("[data-testid=review-failed]"), Is.Empty);
            Assert.That(Context.JSInterop.Invocations.Where(i => i.Identifier == "Blazor._internal.domWrapper.focus"), Is.Empty);
        });
    }

    [Test]
    public void ChooseAnother_RaisesOnChooseAnother()
    {
        var chooses = 0;
        var cut = Context.Render<ImportReviewView>(p => p
            .Add(x => x.Review, ImportReview.Rejected("photo.json", ImportError.NotAnExport)).Add(x => x.OnChooseAnother, () => chooses++));

        cut.Find("[data-testid=review-choose]").Click();

        Assert.That(chooses, Is.EqualTo(1));
    }

    [Test]
    public void ReadingAnotherFile_ShowsChooseBusyAndKeepsCancel()
    {
        var chooses = 0;
        var cut = Context.Render<ImportReviewView>(p => p
            .Add(x => x.Review, ImportReview.Rejected("photo.json", ImportError.NotAnExport))
            .Add(x => x.Reading, true)
            .Add(x => x.OnChooseAnother, () => chooses++));

        var choose = cut.Find("[data-testid=review-choose]");
        choose.Click();

        Assert.Multiple(() =>
        {
            Assert.That(choose.TextContent.Trim(), Is.EqualTo("Reading…"));
            Assert.That(choose.GetAttribute("aria-busy"), Is.EqualTo("true"));
            Assert.That(chooses, Is.Zero);
            Assert.That(cut.Find("[data-testid=review-cancel]").HasAttribute("disabled"), Is.False);
        });
    }

    private static ImportReview Review(int fresh = 0, int already = 0, int different = 0, int skipped = 0) =>
        new(FileName, fresh + already + different + skipped, fresh, already, different, skipped, PhoneCount: 41);

    private static (string Value, string Label) Tile(IRenderedComponent<ImportReviewView> cut, string testId) =>
        (cut.Find($"[data-testid={testId}] .value").TextContent, cut.Find($"[data-testid={testId}] .label").TextContent);

    private IRenderedComponent<ImportReviewView> Render(
        ImportReview review, ImportMode mode = ImportMode.Add, DifferentChoice different = DifferentChoice.KeepPhones, bool failed = false) =>
        Context.Render<ImportReviewView>(p => p
            .Add(x => x.Review, review)
            .Add(x => x.Mode, mode)
            .Add(x => x.Different, different)
            .Add(x => x.Failed, failed)
            .Add(x => x.OnImport, () => { })
            .Add(x => x.OnCancel, () => { })
            .Add(x => x.OnChooseAnother, () => { })
            .Add(x => x.OnBack, () => { }));

    private IReadOnlyList<JSRuntimeInvocation> Focuses() =>
        Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"];
}
