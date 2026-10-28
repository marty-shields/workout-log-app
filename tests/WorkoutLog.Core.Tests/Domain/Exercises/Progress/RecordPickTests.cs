using WorkoutLog.Core.Domain.Exercises.Progress;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Progress;

public class RecordPickTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 20, 17, 0, 0, TimeSpan.Zero);

    [Test]
    public void Highest_IsTheHighestValueWithItsStart() =>
        Assert.That(RecordPick.Highest(Sessions((1, 5), (2, 9), (3, 7)), s => s.Start, s => s.Value),
            Is.EqualTo(new ProgressRecord<int>(9, Start.AddDays(2))));

    [Test]
    public void Lowest_IsTheLowestValueWithItsStart() =>
        Assert.That(RecordPick.Lowest(Sessions((1, 5), (2, 3), (3, 7)), s => s.Start, s => s.Value),
            Is.EqualTo(new ProgressRecord<int>(3, Start.AddDays(2))));

    [Test]
    public void Highest_Tie_TakesTheEarliestStart() =>
        Assert.That(RecordPick.Highest(Sessions((3, 9), (1, 9), (2, 5)), s => s.Start, s => s.Value),
            Is.EqualTo(new ProgressRecord<int>(9, Start.AddDays(1))));

    [Test]
    public void Lowest_Tie_TakesTheEarliestStart() =>
        Assert.That(RecordPick.Lowest(Sessions((3, 2), (1, 2), (2, 5)), s => s.Start, s => s.Value),
            Is.EqualTo(new ProgressRecord<int>(2, Start.AddDays(1))));

    [Test]
    public void Highest_SessionsWithNoValue_AreLeftOut() =>
        Assert.That(RecordPick.Highest(Sessions((1, null), (2, 4)), s => s.Start, s => s.Value),
            Is.EqualTo(new ProgressRecord<int>(4, Start.AddDays(2))));

    [Test]
    public void Highest_NoSessionHasAValue_IsNull() =>
        Assert.That(RecordPick.Highest(Sessions((1, null)), s => s.Start, s => s.Value), Is.Null);

    [Test]
    public void First_UsesTheOrderGiven_AndTakesTheEarliestStartOnATie() =>
        Assert.That(RecordPick.First(
                [(Start.AddDays(3), "bb"), (Start.AddDays(1), "a"), (Start.AddDays(2), "bb"), (Start, (string?)null)],
                s => s.Item1,
                s => s.Item2,
                r => r.OrderByDescending(x => x.Value.Length)),
            Is.EqualTo(new ProgressRecord<string>("bb", Start.AddDays(2))));

    [Test]
    public void First_NoSessionHasAValue_IsNull() =>
        Assert.That(RecordPick.First([(Start, (string?)null)], s => s.Item1, s => s.Item2, r => r.OrderBy(x => x.Value)), Is.Null);

    private static (DateTimeOffset Start, int? Value)[] Sessions(params (int Day, int? Value)[] values) =>
        [.. values.Select(v => (Start.AddDays(v.Day), v.Value))];
}
