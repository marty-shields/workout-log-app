using Microsoft.JSInterop;
using WorkoutLog.UI.Theme;

namespace WorkoutLog.UI.Tests.Theme;

public class SystemBarInsetWriterTests
{
    private const string SetProperty = "document.documentElement.style.setProperty";

    private BunitContext context = null!;
    private SystemBarInsetWriter writer = null!;

    [SetUp]
    public void SetUp()
    {
        context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        writer = new SystemBarInsetWriter(context.JSInterop.JSRuntime);
    }

    [TearDown]
    public void TearDown() => context.Dispose();

    [Test]
    public async Task Apply_ConvertsDevicePixelsToCssPixels()
    {
        await writer.ApplyAsync(136, 126, 2.625f);

        Assert.That(Written(), Is.EqualTo(new[] { "--inset-top=51.809525px", "--inset-bottom=48px" }));
    }

    [Test]
    public async Task Apply_WritesNoInsets_AsZeroPixels()
    {
        await writer.ApplyAsync(0, 0, 3f);

        Assert.That(Written(), Is.EqualTo(new[] { "--inset-top=0px", "--inset-bottom=0px" }));
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    public async Task Apply_WithNoDensity_WritesNothing(float density)
    {
        await writer.ApplyAsync(136, 126, density);

        Assert.That(Written(), Is.Empty);
    }

    [Test]
    public async Task Apply_SameValuesAgain_WritesOnce()
    {
        await writer.ApplyAsync(136, 126, 2.625f);
        await writer.ApplyAsync(136, 126, 2.625f);

        Assert.That(Written(), Has.Count.EqualTo(2));
    }

    [Test]
    public async Task Apply_OtherValues_WritesThemAgain()
    {
        await writer.ApplyAsync(136, 126, 2.625f);
        await writer.ApplyAsync(136, 63, 2.625f);

        Assert.That(Written().Skip(2), Is.EqualTo(new[] { "--inset-top=51.809525px", "--inset-bottom=24px" }));
    }

    [Test]
    public async Task Apply_AfterAFailedWrite_TriesTheSameValuesAgain()
    {
        var failing = new FailingOnce();
        var retrying = new SystemBarInsetWriter(failing);

        await retrying.ApplyAsync(136, 126, 2.625f);
        await retrying.ApplyAsync(136, 126, 2.625f);

        Assert.That(failing.Succeeded, Is.EqualTo(new[] { "--inset-top=51.809525px", "--inset-bottom=48px" }));
    }

    [Test]
    public void Apply_WhenThePageIsGone_DoesNotThrow()
    {
        var gone = new FailingOnce(new JSDisconnectedException("gone"));

        Assert.That(async () => await new SystemBarInsetWriter(gone).ApplyAsync(136, 126, 2.625f), Throws.Nothing);
    }

    private sealed class FailingOnce(Exception? failure = null) : IJSRuntime
    {
        private bool failed;

        public List<string> Succeeded { get; } = [];

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            if (!failed)
            {
                failed = true;
                throw failure ?? new JSException("boom");
            }

            Succeeded.Add($"{args![0]}={args[1]}");
            return default;
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            InvokeAsync<TValue>(identifier, args);
    }

    private List<string> Written() =>
        context.JSInterop.Invocations
            .Where(call => call.Identifier == SetProperty)
            .Select(call => $"{call.Arguments[0]}={call.Arguments[1]}")
            .ToList();
}
