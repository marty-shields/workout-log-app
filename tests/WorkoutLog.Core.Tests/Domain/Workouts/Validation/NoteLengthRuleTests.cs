using WorkoutLog.Core.Domain.Workouts.Validation;

namespace WorkoutLog.Core.Tests.Domain.Workouts.Validation;

public class NoteLengthRuleTests
{
    private readonly NoteLengthRule rule = new();

    [TestCase(1)]
    [TestCase(500)]
    public void Check_UpTo500Characters_Passes(int length) =>
        Assert.That(rule.Check(new string('n', length)), Is.Null);

    [TestCase(501)]
    [TestCase(5000)]
    public void Check_Over500Characters_IsTooLong(int length) =>
        Assert.That(rule.Check(new string('n', length)), Is.EqualTo(NoteError.TooLong));

    [Test]
    public void Check_500CharactersInsideWhitespace_Passes() =>
        Assert.That(rule.Check($"\n  {new string('n', 500)} \t"), Is.Null);

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("\t\r\n")]
    public void Check_NoNote_Passes(string? note) =>
        Assert.That(rule.Check(note), Is.Null);

    [Test]
    public void Check_WhitespaceOnlyLongerThan500_Passes() =>
        Assert.That(rule.Check(new string(' ', 600)), Is.Null);
}
