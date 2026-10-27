using OpenQA.Selenium;

namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>Import Review: the file, its counts, the choices, the file errors and the Replace all prompt.</summary>
public sealed class ImportReviewScreen(WebView web)
{
    public const string Title = "Import workouts";

    public string Total() => web.Text("review-total");

    /// <summary>The four counts: New, Already on this phone, Different on this phone, Can't be imported.</summary>
    public (int New, int Already, int Different, int Skipped) Counts() =>
        (Count("review-new"), Count("review-already"), Count("review-different"), Count("review-skipped"));

    public string? Error() => web.Exists("review-error") ? web.Text("review-error") : null;

    public string? NothingNew() => web.Exists("review-nothing-new") ? web.Text("review-nothing-new") : null;

    public bool ImportEnabled() => web.Find("review-import").Enabled;

    public string ImportText() => web.Text("review-import");

    /// <summary>Taps the row of a choice: the radio inside it is transparent.</summary>
    public void Choose(string testId) => web.Find(testId).FindElement(By.XPath("..")).Click();

    public void Import() => web.Tap("review-import");

    public void Cancel() => web.Tap("review-cancel");

    public void ChooseAnother() => web.Tap("review-choose");

    public bool PromptOpen() => web.Exists("replace-dialog");

    public string PromptTitle() => web.Find("replace-dialog").FindElement(By.CssSelector(".title")).Text.Trim();

    public void Replace() => web.Tap("replace-dialog-confirm");

    private int Count(string testId) => int.Parse(web.Find(testId).FindElement(By.CssSelector(".value")).Text.Trim());
}
