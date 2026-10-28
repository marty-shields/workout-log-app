using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Progress;

public class AssistedRecordsRuleTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 20, 17, 0, 0, TimeSpan.Zero);

    private readonly AssistedRecordsRule rule = new();

    [Test]
    public void Records_NoSessions_AreNone() =>
        Assert.That(rule.Records([]), Is.EqualTo(new AssistedRecords(null)));

    [Test]
    public void LeastAssist_IsTheLowestSessionAssist() =>
        Assert.That(rule.Records([Session(Start.AddDays(-7), Best(5m, 6)), Session(Start, Best(10m, 8))]).LeastAssist,
            Is.EqualTo(new ProgressRecord<ExerciseBest>(Best(5m, 6), Start.AddDays(-7))));

    [Test]
    public void LeastAssist_TiedAssist_TakesTheMostReps() =>
        Assert.That(rule.Records([Session(Start.AddDays(-7), Best(0m, 4)), Session(Start, Best(0m, 5))]).LeastAssist,
            Is.EqualTo(new ProgressRecord<ExerciseBest>(Best(0m, 5), Start)));

    [Test]
    public void LeastAssist_TiedAcrossSessions_TakesTheEarliest() =>
        Assert.That(rule.Records([Session(Start, Best(0m, 5)), Session(Start.AddDays(-7), Best(0m, 5)), Session(Start.AddDays(-3), Best(0m, 5))]).LeastAssist,
            Is.EqualTo(new ProgressRecord<ExerciseBest>(Best(0m, 5), Start.AddDays(-7))));

    [Test]
    public void Records_SessionsWithoutValues_AreNone() =>
        Assert.That(rule.Records([Session(Start, null)]), Is.EqualTo(new AssistedRecords(null)));

    private static ExerciseBest Best(decimal kg, int reps) => new(kg, reps, null, null);

    private static AssistedSessionProgress Session(DateTimeOffset startedAt, ExerciseBest? leastAssist) =>
        new(new StrengthSession(1, startedAt, []), leastAssist);
}
