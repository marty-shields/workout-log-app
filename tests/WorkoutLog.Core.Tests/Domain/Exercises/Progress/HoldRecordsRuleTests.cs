using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Progress;

public class HoldRecordsRuleTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 20, 17, 0, 0, TimeSpan.Zero);

    private readonly HoldRecordsRule rule = new();

    [Test]
    public void Records_NoSessions_AreNone() =>
        Assert.That(rule.Records([]), Is.EqualTo(new HoldRecords(null, null)));

    [Test]
    public void Records_TakeTheLongestHoldAndTheHeaviestWeight() =>
        Assert.That(rule.Records([Session(Start.AddDays(-7), 105, Best(5m, 45)), Session(Start, 60, Best(10m, 60))]),
            Is.EqualTo(new HoldRecords(
                new ProgressRecord<TimeSpan>(TimeSpan.FromSeconds(105), Start.AddDays(-7)),
                new ProgressRecord<ExerciseBest>(Best(10m, 60), Start))));

    [Test]
    public void HeaviestWeight_TiedWeight_TakesTheLongest() =>
        Assert.That(rule.Records([Session(Start.AddDays(-7), 60, Best(10m, 45)), Session(Start, 60, Best(10m, 60))]).HeaviestWeight,
            Is.EqualTo(new ProgressRecord<ExerciseBest>(Best(10m, 60), Start)));

    [Test]
    public void Records_TiedAcrossSessions_TakeTheEarliest() =>
        Assert.That(rule.Records([Session(Start, 90, Best(10m, 60)), Session(Start.AddDays(-7), 90, Best(10m, 60)), Session(Start.AddDays(-3), 90, Best(10m, 60))]),
            Is.EqualTo(new HoldRecords(
                new ProgressRecord<TimeSpan>(TimeSpan.FromSeconds(90), Start.AddDays(-7)),
                new ProgressRecord<ExerciseBest>(Best(10m, 60), Start.AddDays(-7)))));

    [Test]
    public void Records_SessionsWithoutValues_AreNone() =>
        Assert.That(rule.Records([Session(Start, null, null)]), Is.EqualTo(new HoldRecords(null, null)));

    private static ExerciseBest Best(decimal kg, int seconds) => new(kg, null, TimeSpan.FromSeconds(seconds), null);

    private static HoldSessionProgress Session(DateTimeOffset startedAt, int? longestSeconds, ExerciseBest? heaviest) =>
        new(new StrengthSession(1, startedAt, []), longestSeconds is { } s ? TimeSpan.FromSeconds(s) : null, heaviest);
}
