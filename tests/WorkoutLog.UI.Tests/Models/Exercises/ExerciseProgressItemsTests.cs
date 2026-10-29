using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Components.Charts;
using WorkoutLog.UI.Components.Lists;
using WorkoutLog.UI.Models.Exercises;

namespace WorkoutLog.UI.Tests.Models.Exercises;

public class ExerciseProgressItemsTests
{
    private const int Squat = 6;
    private const int PushUp = 264;

    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();

    /// <summary>Wed 28 Oct 2026 12:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    /// <summary>UTC+10, no daylight saving.</summary>
    private static readonly TimeZoneInfo Brisbane = TimeZoneInfo.CreateCustomTimeZone("UTC+10", TimeSpan.FromHours(10), "UTC+10", "UTC+10");

    private static readonly LiftSessionProgress Heavy =
        Session(3, Now.AddDays(-8), [Set(8, 60m), Set(8, 62.5m), Set(3, 70m)], Top(70m, 3), 62.5m * (1 + 8 / 30m));

    private static readonly LiftSessionProgress NoWeight = Session(2, Now.AddDays(-15), [Set(8, null), Set(10, null)], null, null);

    private static readonly LiftSessionProgress LastYear = Session(1, new DateTimeOffset(2025, 10, 15, 9, 0, 0, TimeSpan.Zero), [Set(15, 40m)], Top(40m, 15), null);

    [Test]
    public void Exercises_AreTheOnesNamed() =>
        Assert.That(new[] { Squat, PushUp }.Select(id => Find(id).Name), Is.EqualTo(new[] { "Barbell Back Squat", "Push-up" }));

    [Test]
    public void Tabs_AreTopSetThenE1rm() =>
        Assert.That(ExerciseProgressItems.Tabs.Select(t => (t.Value, t.Label, t.TestId)),
            Is.EqualTo(new[] { ("top-set", "Top set", "exercise-tab-top-set"), ("e1rm", "e1RM", "exercise-tab-e1rm") }));

    [Test]
    public void Subtitle_IsTheMuscleAndEquipment() =>
        Assert.That(ExerciseProgressItems.Subtitle(Find(Squat)), Is.EqualTo("Quadriceps · Barbell"));

    [Test]
    public void Subtitle_EquipmentNone_IsTheMuscleOnly() =>
        Assert.That(ExerciseProgressItems.Subtitle(Find(PushUp)), Is.EqualTo("Chest"));

    [TestCase(Equipment.EzBar, "EZ-Bar")]
    [TestCase(Equipment.SmithMachine, "Smith Machine")]
    [TestCase(Equipment.TrapBar, "Trap Bar")]
    public void Subtitle_EquipmentOfTwoWords_IsSpelledAsInTheNames(Equipment equipment, string text) =>
        Assert.That(ExerciseProgressItems.Subtitle(Catalog.All.First(e => e.Category == ExerciseCategory.Lift && e.Equipment == equipment)),
            Does.EndWith($" · {text}"));

    [TestCase(Muscle.LowerBack, "Lower Back")]
    [TestCase(Muscle.MiddleBack, "Middle Back")]
    public void Subtitle_MuscleOfTwoWords_IsSpelledOut(Muscle muscle, string text) =>
        Assert.That(ExerciseProgressItems.Subtitle(Catalog.All.First(e => e.Category == ExerciseCategory.Lift && e.PrimaryMuscle == muscle)),
            Does.StartWith($"{text} · "));

    [TestCase("top-set", null)]
    [TestCase("e1rm", "Estimated one-rep max")]
    public void Caption_IsForE1rmOnly(string tab, string? expected) =>
        Assert.That(ExerciseProgressItems.Caption(tab), Is.EqualTo(expected));

    [Test]
    public void Points_TopSet_AreTheWeightsLabelledWithTheReps_NoWeightHasNoValue() =>
        Assert.That(ExerciseProgressItems.Points(Progress(LastYear, NoWeight, Heavy), "top-set", TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[] { new ChartPoint("15 Oct 2025", 40m, "×15"), new ChartPoint("13 Oct", null), new ChartPoint("20 Oct", 70m, "×3") }));

    [Test]
    public void Points_E1rm_AreTheUnroundedEstimates_NoneHasNoValue() =>
        Assert.That(ExerciseProgressItems.Points(Progress(LastYear, NoWeight, Heavy), "e1rm", TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[] { new ChartPoint("15 Oct 2025", null), new ChartPoint("13 Oct", null), new ChartPoint("20 Oct", 62.5m * (1 + 8 / 30m)) }));

    [Test]
    public void Points_DatesAreInThePhonesTimeZone() =>
        Assert.That(ExerciseProgressItems.Points(Progress(Session(1, new DateTimeOffset(2026, 10, 20, 15, 0, 0, TimeSpan.Zero), [Set(5, 60m)], Top(60m, 5), 70m)), "top-set", Brisbane, Now)
            .Single().Date, Is.EqualTo("21 Oct"));

    [Test]
    public void Points_TwoSessionsOnOneDay_AreTwoPoints() =>
        Assert.That(ExerciseProgressItems.Points(
                Progress(Session(1, Now.AddHours(-3), [Set(5, 60m)], Top(60m, 5), null), Session(2, Now, [Set(3, 70m)], Top(70m, 3), null)), "top-set", TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[] { new ChartPoint("28 Oct", 60m, "×5"), new ChartPoint("28 Oct", 70m, "×3") }));

    [TestCase("e1rm", 82.25, "82.3kg")]
    [TestCase("e1rm", 82.249, "82.2kg")]
    [TestCase("e1rm", 80, "80kg")]
    [TestCase("top-set", 62.5, "62.5kg")]
    public void Format_IsTheTabsWeightText(string tab, decimal value, string expected) =>
        Assert.That((tab == "e1rm" ? ExerciseProgressItems.E1rm : ExerciseProgressItems.Weight)(value), Is.EqualTo(expected));




    [TestCase(ChartRange.ThreeMonths, "No sets in the last 3 months")]
    [TestCase(ChartRange.SixMonths, "No sets in the last 6 months")]
    [TestCase(ChartRange.Year, "No sets in the last year")]
    [TestCase(ChartRange.All, "No sets yet")]
    public void NoSets_NamesTheRange(ChartRange range, string expected) =>
        Assert.That(ExerciseProgressItems.NoSets(range), Is.EqualTo(expected));

    [Test]
    public void Records_AreTheHeaviestWeightAndBestE1rmWithTheirDates() =>
        Assert.That(ExerciseProgressItems.Records(
                Progress(new LiftRecords(
                    new ProgressRecord<ExerciseBest>(Top(70m, 3), Now.AddDays(-8)),
                    new ProgressRecord<decimal>(62.5m * (1 + 8 / 30m), new DateTimeOffset(2025, 10, 15, 9, 0, 0, TimeSpan.Zero)))),
                TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[] { new RecordRow("Heaviest weight", "70kg × 3", "20 Oct"), new RecordRow("Best e1RM", "79.2kg", "15 Oct 2025") }));

    [Test]
    public void Records_None_HaveNoValues() =>
        Assert.That(ExerciseProgressItems.Records(Progress(), TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[] { new RecordRow("Heaviest weight", null, null), new RecordRow("Best e1RM", null, null) }));

    [Test]
    public void Sessions_AreNewestFirstWithTheTopSetAndEverySet() =>
        Assert.That(ExerciseProgressItems.Sessions(Progress(LastYear, NoWeight, Heavy), TimeZoneInfo.Utc, Now), Is.EqualTo(new[]
        {
            new ExerciseSession(3, "Tue 20 Oct", "70kg × 3", "60×8 · 62.5×8 · 70×3"),
            new ExerciseSession(2, "Tue 13 Oct", "10 reps", "8 reps · 10 reps"),
            new ExerciseSession(1, "Wed 15 Oct 2025", "40kg × 15", "40×15"),
        }));

    [Test]
    public void Sessions_SetWithoutWeight_ShowsItsReps() =>
        Assert.That(ExerciseProgressItems.Sessions(Progress(Session(1, Now, [Set(8, 60m), Set(8, null)], Top(60m, 8), 76m)), TimeZoneInfo.Utc, Now).Single().Detail,
            Is.EqualTo("60×8 · 8 reps"));

    [Test]
    public void Sessions_SetWithoutReps_ShowsItsWeight() =>
        Assert.That(ExerciseProgressItems.Sessions(Progress(Session(1, Now, [Set(null, 60m)], new ExerciseBest(60m, null, null, null), null)), TimeZoneInfo.Utc, Now).Single(),
            Is.EqualTo(new ExerciseSession(1, "Wed 28 Oct", "60kg", "60kg")));

    [Test]
    public void Sessions_SetWithNoValues_ShowsADash() =>
        Assert.That(ExerciseProgressItems.Sessions(Progress(Session(1, Now, [Set(null, null)], null, null)), TimeZoneInfo.Utc, Now).Single(),
            Is.EqualTo(new ExerciseSession(1, "Wed 28 Oct", "—", "—")));

    [Test]
    public void Sessions_DatesAreInThePhonesTimeZone() =>
        Assert.That(ExerciseProgressItems.Sessions(Progress(Session(1, new DateTimeOffset(2026, 10, 20, 15, 0, 0, TimeSpan.Zero), [Set(5, 60m)], Top(60m, 5), 70m)), Brisbane, Now)
            .Single().Date, Is.EqualTo("Wed 21 Oct"));

    private static ExerciseDefinition Find(int id) => Catalog.All.Single(e => e.Id == id);

    private static StrengthSet Set(int? reps, decimal? weightKg) => new(reps, weightKg, null, null);

    private static ExerciseBest Top(decimal kg, int reps) => new(kg, reps, null, null);

    private static LiftSessionProgress Session(int id, DateTimeOffset startedAt, StrengthSet[] sets, ExerciseBest? top, decimal? e1rm) =>
        new(new StrengthSession(id, startedAt, sets), top, e1rm);

    private static LiftProgress Progress(params LiftSessionProgress[] sessions) => new(sessions, new LiftRecords(null, null));

    private static LiftProgress Progress(LiftRecords records) => new([], records);
}
