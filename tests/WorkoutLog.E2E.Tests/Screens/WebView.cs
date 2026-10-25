using System.Collections.ObjectModel;
using NUnit.Framework.Constraints;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;

namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>Finds, taps and types into the app's WebView elements by <c>data-testid</c>, and waits for the page to catch up.</summary>
public sealed class WebView
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan Poll = TimeSpan.FromMilliseconds(200);

    /// <summary>How long a step waits before checking that something didn't happen.</summary>
    private static readonly TimeSpan SettleTime = TimeSpan.FromMilliseconds(800);

    /// <summary>Scrolls the element to the middle of the screen when another element covers its centre.</summary>
    private const string ScrollIfCovered = """
        const el = arguments[0], r = el.getBoundingClientRect();
        if (document.elementFromPoint(r.left + r.width / 2, r.top + r.height / 2) !== el) el.scrollIntoView({ block: 'center' });
        """;

    /// <summary>The element's top when it is the topmost element at its centre, otherwise what is there.</summary>
    private const string TopIfUncovered = """
        const el = arguments[0], r = el.getBoundingClientRect();
        const x = r.left + r.width / 2, y = r.top + r.height / 2;
        const at = document.elementFromPoint(x, y);
        if (at === el) return r.top;
        return `(${Math.round(x)}, ${Math.round(y)}) in ${innerWidth}x${innerHeight}: ` + (at ? `${at.tagName.toLowerCase()}.${at.className} ${at.getAttribute('data-testid') ?? ''}` : 'nothing');
        """;

    private const string FocusedUncovered = """
        const el = document.activeElement;
        if (!el || el === document.body) return false;
        const r = el.getBoundingClientRect();
        if (r.top < 0 || r.left < 0 || r.bottom > innerHeight || r.right > innerWidth) return false;
        return document.elementFromPoint(r.left + r.width / 2, r.top + r.height / 2) === el;
        """;

    internal const string InputHasFocus = "return document.activeElement instanceof HTMLInputElement;";

    /// <summary>Records where the next pointer-down lands and what it hits.</summary>
    private const string RecordTap = """
        window.e2eTap = 'no pointer-down';
        document.addEventListener('pointerdown', e => window.e2eTap =
            `pointer-down at (${Math.round(e.clientX)}, ${Math.round(e.clientY)}) on ${e.target.tagName.toLowerCase()} ${e.target.getAttribute('data-testid') ?? ''}`,
            { once: true, capture: true });
        """;

    /// <summary>Null when the element has focus, otherwise the recorded tap and the focused element.</summary>
    private const string TapResult = """
        const el = arguments[0], r = el.getBoundingClientRect();
        if (document.activeElement === el) return null;
        return `${window.e2eTap}; field at (${Math.round(r.left + r.width / 2)}, ${Math.round(r.top + r.height / 2)}); focus on ${document.activeElement?.getAttribute('data-testid') ?? document.activeElement?.tagName}`;
        """;

    private static AndroidDriver App => AppiumSetup.Driver;

    private static By ByTestId(string testId) => By.CssSelector($"[data-testid=\"{testId}\"]");

    /// <summary>Waits for the element with this <c>data-testid</c>.</summary>
    public IWebElement Find(string testId)
    {
        IWebElement? found = null;
        WaitUntil(() => (found = App.FindElements(ByTestId(testId)).FirstOrDefault()) is not null, $"no element {testId}");
        return found!;
    }

    /// <summary>Elements matching a CSS selector, now.</summary>
    public IReadOnlyCollection<IWebElement> FindAll(string css) => App.FindElements(By.CssSelector(css));

    public bool Exists(string testId) => App.FindElements(ByTestId(testId)).Count > 0;

    /// <summary>The element's text, found and read again if the page re-renders it in between.</summary>
    public string Text(string testId)
    {
        var text = "";
        WaitUntil(() =>
        {
            text = Find(testId).Text.Trim();
            return true;
        }, $"no element {testId}");
        return text;
    }

    public void Tap(string testId) => Find(testId).Click();

    /// <summary>Replaces the input's text one key at a time, so the page sees every value as it is typed.</summary>
    public void Type(string testId, string text)
    {
        var input = Find(testId);
        TapInput(input);
        Clear(input);
        foreach (var key in text)
        {
            input.SendKeys(key.ToString());
        }
    }

    /// <summary>Empties the input with Backspace, so the page sees the change.</summary>
    public void Clear(string testId)
    {
        var input = Find(testId);
        TapInput(input);
        Clear(input);
    }

    public string Value(string testId) => Find(testId).GetDomProperty("value") ?? "";

    public string Attribute(string testId, string name) => Find(testId).GetDomAttribute(name) ?? "";

    /// <summary>The <c>data-testid</c> of the focused element, or empty.</summary>
    public string Focused() =>
        App.ExecuteScript("return document.activeElement?.getAttribute('data-testid') ?? '';") as string ?? "";

    /// <summary>Whether the focused element is inside the viewport and the topmost element at its centre.</summary>
    public bool FocusedInView() => App.ExecuteScript(FocusedUncovered) is true;

    /// <summary>Runs a script that returns an array, as a list.</summary>
    public IReadOnlyList<object> List(string script) => (ReadOnlyCollection<object>)App.ExecuteScript(script)!;

    /// <summary>Runs a script that returns an array of strings.</summary>
    public IReadOnlyList<string> Strings(string script) => List(script).Select(o => (string)o).ToList();

    /// <summary>Waits briefly, before checking that something didn't change.</summary>
    public void Settle() => Thread.Sleep(SettleTime);

    /// <summary>Waits until <paramref name="read"/> meets <paramref name="expected"/>, then asserts it.</summary>
    public void Eventually<T>(Func<T> read, IResolveConstraint expected, string? message = null)
    {
        var constraint = expected.Resolve();
        var deadline = DateTime.UtcNow + Timeout;
        while (true)
        {
            try
            {
                var actual = read();
                if (constraint.ApplyTo(actual).IsSuccess || DateTime.UtcNow > deadline)
                {
                    Assert.That(actual, constraint, message);
                    return;
                }
            }
            catch (WebDriverException) when (DateTime.UtcNow < deadline)
            {
                // The element changed while it was read; read it again.
            }

            Thread.Sleep(Poll);
        }
    }

    /// <summary>Waits until <paramref name="condition"/> holds; fails with <paramref name="failure"/> on timeout.</summary>
    public void WaitUntil(Func<bool> condition, string failure, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? Timeout);
        while (true)
        {
            try
            {
                if (condition())
                {
                    return;
                }
            }
            catch (WebDriverException) when (DateTime.UtcNow < deadline)
            {
                // The element changed while it was read; read it again.
            }

            if (DateTime.UtcNow > deadline)
            {
                Assert.Fail($"Timed out: {failure}.");
            }

            Thread.Sleep(Poll);
        }
    }

    /// <summary>
    /// Moves focus to the input. When another input has focus, focus moves by script: with the soft keyboard open, Android
    /// shifts the window and a driver tap lands below its target. Otherwise the input is tapped once it is uncovered and still;
    /// if another element (a bar, or off screen) covers its centre, it is first scrolled to the middle of the screen.
    /// </summary>
    private void TapInput(IWebElement input)
    {
        if (App.ExecuteScript(InputHasFocus) is true)
        {
            App.ExecuteScript("arguments[0].focus();", input);
            Assert.That(App.ExecuteScript(TapResult, input), Is.Null, $"focus didn't move to {input.GetDomAttribute("data-testid")}");
            return;
        }

        App.ExecuteScript(ScrollIfCovered, input);
        var deadline = DateTime.UtcNow + Timeout;
        object? last = null;
        while (true)
        {
            var top = App.ExecuteScript(TopIfUncovered, input);
            if (top is not string && Equals(top, last))
            {
                break;
            }

            if (DateTime.UtcNow > deadline)
            {
                Assert.Fail($"Timed out: {input.GetDomAttribute("data-testid")} didn't come into view; at its centre {top}.");
            }

            last = top;
            Thread.Sleep(Poll);
        }

        App.ExecuteScript(RecordTap);
        input.Click();
        var state = App.ExecuteScript(TapResult, input) as string;
        if (state is not null)
        {
            Assert.Fail($"The tap on {input.GetDomAttribute("data-testid")} missed: {state}.");
        }
    }

    private static void Clear(IWebElement input)
    {
        var length = (input.GetDomProperty("value") ?? "").Length;
        if (length == 0)
        {
            return;
        }

        input.SendKeys(Keys.End);
        for (var i = 0; i < length; i++)
        {
            input.SendKeys(Keys.Backspace);
        }
    }
}
