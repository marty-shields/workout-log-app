using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Tests.Domain.Exercises;

public class SearchTextTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("  - ' ")]
    public void Words_NothingToSearch_IsEmpty(string? text) =>
        Assert.That(SearchText.Words(text), Is.Empty);

    [Test]
    public void Words_MixedCase_IsLowerCase() =>
        Assert.That(SearchText.Words("Bench PRESS"), Is.EqualTo(new[] { "bench", "press" }));

    [TestCase("farmer's walk", new[] { "farmers", "walk" })]
    [TestCase("farmer’s walk", new[] { "farmers", "walk" })]
    public void Words_Apostrophes_AreRemoved(string text, string[] expected) =>
        Assert.That(SearchText.Words(text), Is.EqualTo(expected));

    [TestCase("T-bar row", new[] { "t", "bar", "row" })]
    [TestCase("a,b;;c", new[] { "a", "b", "c" })]
    [TestCase("45 degree", new[] { "45", "degree" })]
    public void Words_OtherCharacters_SeparateWords(string text, string[] expected) =>
        Assert.That(SearchText.Words(text), Is.EqualTo(expected));

    [TestCase("triceps", "tricep")]
    [TestCase("biceps", "bicep")]
    [TestCase("flies", "fly")]
    [TestCase("flyes", "fly")]
    [TestCase("flye", "fly")]
    [TestCase("nordics", "nordic")]
    [TestCase("ups", "up")]
    public void Words_OneWordVariant_IsReplaced(string text, string expected) =>
        Assert.That(SearchText.Words(text), Is.EqualTo(new[] { expected }));

    [TestCase("pullup", new[] { "pull", "up" })]
    [TestCase("Pullups", new[] { "pull", "up" })]
    [TestCase("pushups", new[] { "push", "up" })]
    [TestCase("chinup", new[] { "chin", "up" })]
    [TestCase("situps", new[] { "sit", "up" })]
    [TestCase("stepup", new[] { "step", "up" })]
    [TestCase("skullcrusher", new[] { "skull", "crusher" })]
    public void Words_TwoWordVariant_IsSplit(string text, string[] expected) =>
        Assert.That(SearchText.Words(text), Is.EqualTo(expected));

    [TestCase("pullu", new[] { "pull", "u" })]
    [TestCase("pullup", new[] { "pull", "up" })]
    [TestCase("skullcr", new[] { "skull", "cr" })]
    [TestCase("pushu", new[] { "push", "u" })]
    public void Words_PartlyTypedTwoWordVariant_IsSplitAsTheFullOneWould(string text, string[] expected) =>
        Assert.That(SearchText.Words(text), Is.EqualTo(expected));

    [TestCase("pull")]
    [TestCase("pul")]
    [TestCase("push")]
    [TestCase("skull")]
    [TestCase("pullover")]
    public void Words_FirstPartOnlyOrUnrelated_IsUnchanged(string text) =>
        Assert.That(SearchText.Words(text), Is.EqualTo(new[] { text }));

    [Test]
    public void Words_VariantInsideAPhrase_IsReplacedInPlace() =>
        Assert.That(SearchText.Words("Cable triceps pushups"), Is.EqualTo(new[] { "cable", "tricep", "push", "up" }));
}
