namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>The confirm dialog: Discard, Discard changes and Delete prompts.</summary>
public sealed class ConfirmDialog(WebView web)
{
    public bool IsOpen() => web.Exists("confirm-dialog");

    public string Title() => web.Find("confirm-dialog").FindElement(OpenQA.Selenium.By.CssSelector(".title")).Text.Trim();

    public string CancelText() => web.Text("confirm-dialog-cancel");

    public string ConfirmText() => web.Text("confirm-dialog-confirm");

    public void Cancel() => web.Tap("confirm-dialog-cancel");

    public void Confirm() => web.Tap("confirm-dialog-confirm");
}
