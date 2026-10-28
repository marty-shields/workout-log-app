using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Progress;

public class CarryRecordsRuleTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 20, 17, 0, 0, TimeSpan.Zero);

    private readonly CarryRecordsRule rule = new();

    [Test]
    public void Records_NoSessions_AreNone() =>
        Assert.That(rule.Records([]), Is.EqualTo(new CarryRecords(null, null)));

    [Test]
    public void Records_TakeTheHeaviestWeightAndTheLongestDistance() =>
        Assert.That(rule.Records([Session(Start.AddDays(-7), Best(24m, 40m), Best(24m, 60m)), Session(Start, Best(32m, 40m), Best(32m, 40m))]),
            Is.EqualTo(new CarryRecords(
                new ProgressRecord<ExerciseBest>(Best(32m, 40m), Start),
                new ProgressRecord<ExerciseBest>(Best(24m, 60m), Start.AddDays(-7)))));

    [Test]
    public void HeaviestWeight_TiedWeight_TakesTheLongest() =>
        Assert.That(rule.Records([Session(Start.AddDays(-7), Best(32m, 30m), null), Session(Start, Best(32m, 40m), null)]).HeaviestWeight,
            Is.EqualTo(new ProgressRecord<ExerciseBest>(Best(32m, 40m), Start)));

    [Test]
    public void LongestDistance_TiedDistance_TakesTheHeaviest() =>
        Assert.That(rule.Records([Session(Start.AddDays(-7), null, Best(28m, 60m)), Session(Start, null, Best(null, 60m)), Session(Start.AddDays(-3), null, Best(30m, 60m))]).LongestDistance,
            Is.EqualTo(new ProgressRecord<ExerciseBest>(Best(30m, 60m), Start.AddDays(-3))));

    [Test]
    public void Records_TiedAcrossSessions_TakeTheEarliest() =>
        Assert.That(rule.Records([Session(Start, Best(32m, 40m), Best(32m, 40m)), Session(Start.AddDays(-7), Best(32m, 40m), Best(32m, 40m)), Session(Start.AddDays(-3), Best(32m, 40m), Best(32m, 40m))]),
            Is.EqualTo(new CarryRecords(
                new ProgressRecord<ExerciseBest>(Best(32m, 40m), Start.AddDays(-7)),
                new ProgressRecord<ExerciseBest>(Best(32m, 40m), Start.AddDays(-7)))));

    [Test]
    public void Records_SessionsWithoutValues_AreNone() =>
        Assert.That(rule.Records([Session(Start, null, null)]), Is.EqualTo(new CarryRecords(null, null)));

    private static ExerciseBest Best(decimal? kg, decimal metres) => new(kg, null, null, metres);

    private static CarrySessionProgress Session(DateTimeOffset startedAt, ExerciseBest? heaviest, ExerciseBest? longest) =>
        new(new StrengthSession(1, startedAt, []), heaviest, longest);
}
