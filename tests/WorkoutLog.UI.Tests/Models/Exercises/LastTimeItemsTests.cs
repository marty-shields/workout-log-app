using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Models.Exercises;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Tests.Models.Exercises;

public class LastTimeItemsTests
{
    private const int Squat = 6;
    private const int PushUp = 264;
    private const int AssistedPullUp = 218;
    private const int Plank = 258;
    private const int FarmersWalk = 187;
    private const int Treadmill = 300;
    private const int OutdoorRun = 311;

    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();

    [Test]
    public void Exercises_AreTheOnesNamed() =>
        Assert.That(new[] { Squat, PushUp, AssistedPullUp, Plank, FarmersWalk, Treadmill, OutdoorRun }.Select(id => Find(id).Name),
            Is.EqualTo(new[] { "Barbell Back Squat", "Push-up", "Machine Assisted Pull-up", "Plank", "Farmer's Walk", "Treadmill", "Outdoor Run" }));

    [Test]
    public void From_Lift_IsOneTextPerSetInOrder() =>
        Assert.That(From(Squat, Sets(Set(8, 60m), Set(8, 60m), Set(6, 62.5m)))!.Entries, Is.EqualTo(new[] { "60×8", "60×8", "62.5×6" }));

    [Test]
    public void From_LiftSetsWithoutAValue_AreWrittenAsOnTheExercisePage() =>
        Assert.That(From(Squat, Sets(Set(null, 60m), Set(8, null), Set(null, null)))!.Entries, Is.EqualTo(new[] { "60kg", "8 reps", "—" }));

    [Test]
    public void From_Bodyweight_IsRepsWithTheExtraWeight() =>
        Assert.That(From(PushUp, Sets(Set(15, 0m), Set(12, 10m), Set(10, null)))!.Entries, Is.EqualTo(new[] { "15", "10×12", "10" }));

    [Test]
    public void From_Assisted_IsTheAssistanceWithTheReps() =>
        Assert.That(From(AssistedPullUp, Sets(Set(10, 25m), Set(8, 0m)))!.Entries, Is.EqualTo(new[] { "25kg×10", "0kg×8" }));

    [Test]
    public void From_UnweightedHold_IsTheDuration() =>
        Assert.That(From(Plank, Sets(Hold(0m, 90), Hold(null, 60)))!.Entries, Is.EqualTo(new[] { "1:30", "1:00" }));

    [Test]
    public void From_WeightedHold_IsTheWeightThenTheDurationJoinedByADot() =>
        Assert.That(From(Plank, Sets(Hold(20m, 90), Hold(0m, 60), Hold(2.5m, 45)))!.Entries, Is.EqualTo(new[] { "20kg · 1:30", "1:00", "2.5kg · 0:45" }));

    [Test]
    public void From_HoldWithNoValue_IsADash() =>
        Assert.That(From(Plank, Sets(Hold(0m, null)))!.Entries, Is.EqualTo(new[] { "—" }));

    [Test]
    public void From_Carry_IsWeightDistanceAndDurationJoinedByADot() =>
        Assert.That(From(FarmersWalk, Sets(Carry(32m, 40m, 45), Carry(0m, 60m, null), Carry(24m, 40m, null)))!.Entries,
            Is.EqualTo(new[] { "32kg · 40m · 0:45", "60m", "24kg · 40m" }));

    [Test]
    public void From_Cardio_IsOneEntryWithItsValuesJoinedByADot()
    {
        var entry = new CardioEntry(new TimeSpan(0, 30, 0), 5.2m, null, null, null, null, null);

        var last = From(OutdoorRun, new CardioExercise(OutdoorRun, entry), Best(entry));

        Assert.That(last!.Entries, Is.EqualTo(new[] { "5.2km · 30:00 · 5:46/km" }));
    }

    [Test]
    public void From_CardioWithEveryValue_KeepsTheExercisePagesOrder()
    {
        var entry = new CardioEntry(new TimeSpan(0, 20, 0), 3m, null, null, 1.5m, 190, null);

        var last = From(Treadmill, new CardioExercise(Treadmill, entry), Best(entry));

        Assert.That(last!.Entries, Is.EqualTo(new[] { "3km · 20:00 · 9 km/h · 190 calories · 1.5%" }));
    }

    [Test]
    public void From_CardioWithOnlyADuration_IsTheDuration()
    {
        var entry = new CardioEntry(new TimeSpan(0, 25, 30), null, null, null, null, null, null);

        Assert.That(From(Treadmill, new CardioExercise(Treadmill, entry), Best(entry))!.Entries, Is.EqualTo(new[] { "25:30" }));
    }

    [Test]
    public void From_NoLastSession_IsNull() =>
        Assert.That(LastTimeItems.From(Find(Squat), new LastSessionResult(null, null, Failed: false)), Is.Null);

    [Test]
    public void From_FailedRead_IsNull() =>
        Assert.That(LastTimeItems.From(Find(Squat), new LastSessionResult(null, null, Failed: true)), Is.Null);

    [Test]
    public void From_CardioSessionOfAStrengthExercise_IsNull() =>
        Assert.That(From(Squat, new CardioExercise(Squat, new CardioEntry(new TimeSpan(0, 30, 0), null, null, null, null, null, null)),
            new CardioBest(null, new TimeSpan(0, 30, 0), null, null, null, null)), Is.Null);

    [Test]
    public void From_StrengthSessionOfACardioExercise_IsNull() =>
        Assert.That(From(Treadmill, Sets(Set(8, 60m))), Is.Null);

    private static ExerciseDefinition Find(int id) => Catalog.All.Single(e => e.Id == id);

    private static LastTime? From(int exerciseId, WorkoutExercise exercise, CardioBest? best = null) =>
        LastTimeItems.From(Find(exerciseId), new LastSessionResult(exercise, best, Failed: false));

    private static StrengthExercise Sets(params StrengthSet[] sets) => new(0, sets);

    private static StrengthSet Set(int? reps, decimal? kg) => new(reps, kg, null, null);

    private static StrengthSet Hold(decimal? kg, int? seconds) => new(null, kg, seconds is { } s ? TimeSpan.FromSeconds(s) : null, null);

    private static StrengthSet Carry(decimal? kg, decimal? metres, int? seconds) =>
        new(null, kg, seconds is { } s ? TimeSpan.FromSeconds(s) : null, metres);

    private static CardioBest Best(CardioEntry entry) =>
        new(entry.Distance, entry.Duration, WorkedOut.Pace(entry.Duration, entry.Distance), WorkedOut.Speed(entry.Duration, entry.Distance), entry.Floors, entry.Calories);
}
