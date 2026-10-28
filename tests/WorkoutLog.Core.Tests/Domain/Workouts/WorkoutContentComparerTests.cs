using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Tests.Domain.Workouts;

public class WorkoutContentComparerTests
{
    /// <summary>Machine Assisted Pull-up: weight labelled Assist.</summary>
    private const int AssistedPullUp = 218;

    private static readonly DateTimeOffset Start = new DateTimeOffset(2026, 10, 21, 17, 5, 0, TimeSpan.Zero).AddTicks(1234567);
    private static readonly DateTimeOffset Finish = Start.AddMinutes(62);

    private readonly WorkoutContentComparer comparer = new(new ExerciseCatalog());

    [Test]
    public void AreSame_SameContents_IsTrue() =>
        Assert.That(comparer.AreSame(Saved(Exercises()), Imported(Exercises())), Is.True);

    [Test]
    public void AreSame_NoExercises_IsTrue() =>
        Assert.That(comparer.AreSame(Saved([]), Imported([])), Is.True);

    [Test]
    public void AreSame_StartIsNotCompared() =>
        Assert.That(comparer.AreSame(Saved(Exercises()), Imported(Exercises()) with { StartedAt = Start.AddDays(1) }), Is.True);

    [Test]
    public void AreSame_FinishATickApart_IsFalse() =>
        Assert.That(comparer.AreSame(Saved(Exercises()), Imported(Exercises()) with { FinishedAt = Finish.AddTicks(1) }), Is.False);

    [Test]
    public void AreSame_FinishInAnotherOffset_IsTrue() =>
        Assert.That(comparer.AreSame(Saved(Exercises()), Imported(Exercises()) with { FinishedAt = Finish.ToOffset(TimeSpan.FromHours(2)) }), Is.True);

    [Test]
    public void AreSame_DecimalsThatRoundTheSame_IsTrue() =>
        Assert.That(comparer.AreSame(
            Saved([new StrengthExercise(1, [new StrengthSet(5, 22.505m, null, 1.004m)])]),
            Imported([new StrengthExercise(1, [new StrengthSet(5, 22.51m, null, 1m)])])), Is.True);

    [Test]
    public void AreSame_DecimalsThatRoundDifferently_IsFalse() =>
        Assert.That(comparer.AreSame(
            Saved([new StrengthExercise(1, [new StrengthSet(5, 22.504m, null, null)])]),
            Imported([new StrengthExercise(1, [new StrengthSet(5, 22.51m, null, null)])])), Is.False);

    [Test]
    public void AreSame_DurationsInTheSameSecond_IsTrue() =>
        Assert.That(comparer.AreSame(
            Saved([new CardioExercise(300, Entry() with { Duration = new TimeSpan(0, 25, 30).Add(TimeSpan.FromMilliseconds(900)) })]),
            Imported([new CardioExercise(300, Entry())])), Is.True);

    [Test]
    public void AreSame_DurationsInDifferentSeconds_IsFalse() =>
        Assert.That(comparer.AreSame(
            Saved([new CardioExercise(300, Entry() with { Pace = new TimeSpan(0, 5, 31) })]),
            Imported([new CardioExercise(300, Entry())])), Is.False);

    [TestCase(0, null)]
    [TestCase(null, 0)]
    public void AreSame_BlankAndZeroAssist_IsTrue(double? saved, double? imported) =>
        Assert.That(comparer.AreSame(
            Saved([new StrengthExercise(AssistedPullUp, [new StrengthSet(8, (decimal?)saved, null, null)])]),
            Imported([new StrengthExercise(AssistedPullUp, [new StrengthSet(8, (decimal?)imported, null, null)])])), Is.True);

    [Test]
    public void AreSame_BlankAndNonZeroAssist_IsFalse() =>
        Assert.That(comparer.AreSame(
            Saved([new StrengthExercise(AssistedPullUp, [new StrengthSet(8, 25m, null, null)])]),
            Imported([new StrengthExercise(AssistedPullUp, [new StrengthSet(8, null, null, null)])])), Is.False);

    [Test]
    public void AreSame_BlankAndZeroWeightNotAssisted_IsFalse() =>
        Assert.That(comparer.AreSame(
            Saved([new StrengthExercise(1, [new StrengthSet(5, 0m, null, null)])]),
            Imported([new StrengthExercise(1, [new StrengthSet(5, null, null, null)])])), Is.False);

    [TestCase("Order")]
    [TestCase("Exercise id")]
    [TestCase("Extra exercise")]
    [TestCase("Strength vs cardio")]
    [TestCase("Set order")]
    [TestCase("Extra set")]
    [TestCase("Reps")]
    [TestCase("Weight missing")]
    [TestCase("Floors")]
    [TestCase("Level")]
    [TestCase("Incline")]
    [TestCase("Calories")]
    [TestCase("Rate")]
    [TestCase("Distance")]
    public void AreSame_ContentsDiffer_IsFalse(string change)
    {
        var exercises = Exercises();
        var bench = (StrengthExercise)exercises[0];
        var run = (CardioExercise)exercises[1];
        WorkoutExercise[] changed = change switch
        {
            "Order" => [run, bench],
            "Exercise id" => [bench with { ExerciseId = 2 }, run],
            "Extra exercise" => [bench, run, bench],
            "Strength vs cardio" => [bench, new StrengthExercise(300, [])],
            "Set order" => [bench with { Sets = bench.Sets.Reverse().ToList() }, run],
            "Extra set" => [bench with { Sets = [.. bench.Sets, bench.Sets[0]] }, run],
            "Reps" => [bench with { Sets = [bench.Sets[0] with { Reps = 11 }, bench.Sets[1]] }, run],
            "Weight missing" => [bench with { Sets = [bench.Sets[0] with { WeightKg = null }, bench.Sets[1]] }, run],
            "Floors" => [bench, run with { Entry = run.Entry with { Floors = 13 } }],
            "Level" => [bench, run with { Entry = run.Entry with { Level = 9 } }],
            "Incline" => [bench, run with { Entry = run.Entry with { Incline = 2m } }],
            "Calories" => [bench, run with { Entry = run.Entry with { Calories = null } }],
            "Rate" => [bench, run with { Entry = run.Entry with { Rate = 6m } }],
            _ => [bench, run with { Entry = run.Entry with { Distance = 5.21m } }],
        };

        Assert.That(comparer.AreSame(Saved(Exercises()), Imported(changed)), Is.False);
    }

    private static Workout Saved(IReadOnlyList<WorkoutExercise> exercises) => new(1, Start, Finish, exercises);

    private static NewWorkout Imported(IReadOnlyList<WorkoutExercise> exercises) => new(Start, Finish, exercises);

    private static WorkoutExercise[] Exercises() =>
    [
        new StrengthExercise(1, [new StrengthSet(10, 22.75m, null, null), new StrengthSet(8, 25m, null, null)]),
        new CardioExercise(300, Entry()),
    ];

    private static CardioEntry Entry() => new(new TimeSpan(0, 25, 30), 5.2m, 12, 8, 1.5m, 310, 5.5m, new TimeSpan(0, 5, 30));
}
