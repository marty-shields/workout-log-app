using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Tests.Domain.Workouts;

public class NoteTextTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("\t\r\n ")]
    public void Normalize_BlankNote_IsNull(string? note) =>
        Assert.That(NoteText.Normalize(note), Is.Null);

    [TestCase("Felt strong", ExpectedResult = "Felt strong")]
    [TestCase("  Felt strong \n", ExpectedResult = "Felt strong")]
    [TestCase("Line one\nline two ", ExpectedResult = "Line one\nline two")]
    public string? Normalize_Note_IsTrimmed(string note) => NoteText.Normalize(note);
}
