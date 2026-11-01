using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using static WorkoutLog.Core.Tests.Domain.Exercises.Bests.BestRuleCases;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Bests;

public class RecordCheckTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 1, 18, 0, 0, TimeSpan.Zero);

    private readonly RecordCheck check = new(
    [
        new LiftBestRule(new TopSetRule()), new BodyweightBestRule(new BodyweightRule(new TopSetRule(), new E1rmRule())), new AssistedBestRule(new LeastAssistRule()),
        new HoldBestRule(new HoldRule()), new CarryBestRule(new CarryRule()), new CardioBestRule(new CardioSessionRule()),
    ]);

    [Test]
    public void Records_NoSessions_IsEmpty() =>
        Assert.That(check.Records(Definition(Squat), []), Is.Empty);

    [Test]
    public void Records_FirstSession_IsNotARecord() =>
        Assert.That(check.Records(Definition(Squat), Sessions(Sets(Squat, Set(5, 100m)))), Is.Empty);

    [Test]
    public void Records_Tie_IsNotARecord() =>
        Assert.That(check.Records(Definition(Squat), Sessions(Sets(Squat, Set(5, 60m)), Sets(Squat, Set(5, 60m)))), Is.Empty);

    [Test]
    public void Records_EachSessionThatBeatsEveryEarlierOne_OldestFirst()
    {
        var sessions = Sessions(
            Sets(Squat, Set(5, 60m)),
            Sets(Squat, Set(3, 70m)),
            Sets(Squat, Set(8, 65m)),
            Sets(Squat, Set(4, 70m)),
            Sets(Squat, Set(1, 80m)));

        Assert.That(check.Records(Definition(Squat), sessions), Is.EqualTo(new[]
        {
            new SessionRecord(sessions[1], new ExerciseBest(70m, 3, null, null)),
            new SessionRecord(sessions[3], new ExerciseBest(70m, 4, null, null)),
            new SessionRecord(sessions[4], new ExerciseBest(80m, 1, null, null)),
        }));
    }

    [Test]
    public void Records_ComparesByStartTimeNotListOrder()
    {
        var sessions = Sessions(Sets(Squat, Set(5, 60m)), Sets(Squat, Set(5, 70m)));

        Assert.That(check.Records(Definition(Squat), sessions.Reverse()).Select(r => r.Session), Is.EqualTo(new[] { sessions[1] }));
    }

    [Test]
    public void Records_SessionsWithNoBest_AreSkipped()
    {
        var sessions = Sessions(Sets(Squat, Set(10, null)), Sets(Squat, Set(5, 60m)), Sets(Squat, Set(12, null)), Sets(Squat, Set(5, 65m)));

        Assert.That(check.Records(Definition(Squat), sessions).Select(r => r.Session), Is.EqualTo(new[] { sessions[3] }));
    }

    [Test]
    public void Records_Bodyweight_MoreUnweightedRepsOrAWeightedSet()
    {
        var sessions = Sessions(Sets(PushUp, Set(20, 0m)), Sets(PushUp, Set(25, 0m)), Sets(PushUp, Set(5, 10m)), Sets(PushUp, Set(30, 0m)));

        Assert.That(check.Records(Definition(PushUp), sessions).Select(r => r.Best), Is.EqualTo(new[]
        {
            new ExerciseBest(null, 25, null, null),
            new ExerciseBest(10m, 5, null, null),
        }));
    }

    [Test]
    public void Records_Assisted_LessAssistIsBetter()
    {
        var sessions = Sessions(Sets(AssistedPullUp, Set(10, 25m)), Sets(AssistedPullUp, Set(10, 30m)), Sets(AssistedPullUp, Set(8, 20m)));

        Assert.That(check.Records(Definition(AssistedPullUp), sessions).Select(r => r.Best), Is.EqualTo(new[] { new ExerciseBest(20m, 8, null, null) }));
    }

    [Test]
    public void Records_Hold_Longest()
    {
        var sessions = Sessions(Sets(Plank, Set(null, 0m, TimeSpan.FromSeconds(90))), Sets(Plank, Set(null, 0m, TimeSpan.FromSeconds(105))));

        Assert.That(check.Records(Definition(Plank), sessions).Select(r => r.Best),
            Is.EqualTo(new[] { new ExerciseBest(null, null, TimeSpan.FromSeconds(105), null) }));
    }

    [Test]
    public void Records_Carry_Heaviest()
    {
        var sessions = Sessions(
            Sets(FarmersWalk, Set(null, 28m, TimeSpan.FromSeconds(45), 60m)),
            Sets(FarmersWalk, Set(null, 32m, TimeSpan.FromSeconds(45), 40m)));

        Assert.That(check.Records(Definition(FarmersWalk), sessions).Select(r => r.Best), Is.EqualTo(new[] { new ExerciseBest(32m, null, null, 40m) }));
    }

    [Test]
    public void Records_CardioWithDistance_Farthest()
    {
        var sessions = Sessions(Entry(Run, TimeSpan.FromMinutes(30), 5m), Entry(Run, TimeSpan.FromMinutes(20), 5.2m));

        Assert.That(check.Records(Definition(Run), sessions).Select(r => r.Best), Is.EqualTo(new[] { new ExerciseBest(null, null, null, 5.2m) }));
    }

    [Test]
    public void Records_CardioWithoutDistance_Longest()
    {
        var sessions = Sessions(Entry(JumpRope, TimeSpan.FromMinutes(10), null), Entry(JumpRope, TimeSpan.FromMinutes(12), null));

        Assert.That(check.Records(Definition(JumpRope), sessions).Select(r => r.Best),
            Is.EqualTo(new[] { new ExerciseBest(null, null, TimeSpan.FromMinutes(12), null) }));
    }

    /// <summary>One session per exercise block, a day apart, oldest first.</summary>
    private static WorkoutSession[] Sessions(params WorkoutExercise[] logged) =>
        logged.Select((e, i) => new WorkoutSession(i + 1, Start.AddDays(i), e)).ToArray();
}
