using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Models.Exercises;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Tests.Models.Exercises;

public class ExerciseListItemsTests
{
    private const int Squat = 6;
    private const int BarbellCurl = 15;
    private const int DumbbellCurl = 132;
    private const int FarmersWalk = 187;
    private const int Plank = 258;
    private const int Run = 311;
    private const int BodyweightSquat = 291;

    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();

    /// <summary>Wed 28 Oct 2026 12:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    /// <summary>UTC+10, no daylight saving.</summary>
    private static readonly TimeZoneInfo Brisbane = TimeZoneInfo.CreateCustomTimeZone("UTC+10", TimeSpan.FromHours(10), "UTC+10", "UTC+10");

    [Test]
    public void Exercises_AreTheOnesNamed() =>
        Assert.That(new[] { Squat, BarbellCurl, DumbbellCurl, FarmersWalk, Plank, Run, BodyweightSquat }.Select(id => Find(id).Name),
            Is.EqualTo(new[] { "Barbell Back Squat", "Barbell Curl", "Dumbbell Curl", "Farmer's Walk", "Plank", "Outdoor Run", "Squat" }));

    [Test]
    public void Item_NotLogged_HasNoLast() =>
        Assert.That(ExerciseListItems.Item(Find(Run), null, TimeZoneInfo.Utc, Now), Is.EqualTo(new ExerciseListItem(Run, "Outdoor Run", ExerciseKind.Cardio, null)));

    [Test]
    public void Item_Logged_HasTheDateAndBest() =>
        Assert.That(ExerciseListItems.Item(Find(Squat), Logged(Squat, Now.AddDays(-8), new ExerciseBest(70m, 3, null, null)), TimeZoneInfo.Utc, Now),
            Is.EqualTo(new ExerciseListItem(Squat, "Barbell Back Squat", ExerciseKind.Strength, "Tue 20 Oct · 70kg × 3", Now.AddDays(-8))));

    [Test]
    public void Last_NoBest_IsTheDateOnly() =>
        Assert.That(ExerciseListItems.Last(Find(Squat), Logged(Squat, Now.AddDays(-8), null), TimeZoneInfo.Utc, Now), Is.EqualTo("Tue 20 Oct"));

    [Test]
    public void Last_AnotherYear_HasTheYear() =>
        Assert.That(ExerciseListItems.Last(Find(Squat), Logged(Squat, new DateTimeOffset(2025, 10, 15, 9, 0, 0, TimeSpan.Zero), null), TimeZoneInfo.Utc, Now),
            Is.EqualTo("Wed 15 Oct 2025"));

    [Test]
    public void Last_IsInThePhonesTimeZone() =>
        Assert.That(ExerciseListItems.Last(Find(Squat), Logged(Squat, new DateTimeOffset(2026, 10, 20, 20, 0, 0, TimeSpan.Zero), null), Brisbane, Now),
            Is.EqualTo("Wed 21 Oct"));

    [Test]
    public void Last_YearIsThePhonesYear() =>
        Assert.That(ExerciseListItems.Last(Find(Squat), Logged(Squat, new DateTimeOffset(2025, 12, 31, 20, 0, 0, TimeSpan.Zero), null), Brisbane,
                new DateTimeOffset(2025, 12, 31, 21, 0, 0, TimeSpan.Zero)),
            Is.EqualTo("Thu 1 Jan"));

    [TestCase(Squat, "70", "3", null, null, "70kg × 3", TestName = "Best_TopSet")]
    [TestCase(Squat, "62.50", "8", null, null, "62.5kg × 8", TestName = "Best_TopSet_NoTrailingZeros")]
    [TestCase(Squat, null, "25", null, null, "25 reps", TestName = "Best_Reps")]
    [TestCase(FarmersWalk, "32", null, null, "40", "32kg · 40m", TestName = "Best_Carry")]
    [TestCase(Plank, null, null, 105, null, "1:45", TestName = "Best_Hold")]
    [TestCase(Run, null, null, null, "5.2", "5.2km", TestName = "Best_Distance")]
    [TestCase(Run, null, null, 1800, null, "30:00", TestName = "Best_Duration")]
    public void Best_AsText(int id, string? kg, string? reps, int? seconds, string? distance, string expected) =>
        Assert.That(ExerciseListItems.Best(Find(id), new ExerciseBest(
                kg is null ? null : decimal.Parse(kg, System.Globalization.CultureInfo.InvariantCulture),
                reps is null ? null : int.Parse(reps, System.Globalization.CultureInfo.InvariantCulture),
                seconds is null ? null : TimeSpan.FromSeconds(seconds.Value),
                distance is null ? null : decimal.Parse(distance, System.Globalization.CultureInfo.InvariantCulture))),
            Is.EqualTo(expected));

    [Test]
    public void Best_WeightOnly_IsNull() =>
        Assert.That(ExerciseListItems.Best(Find(Squat), new ExerciseBest(70m, null, null, null)), Is.Null);

    [Test]
    public void Best_DistanceForAnExerciseWithoutADistanceUnit_IsNull() =>
        Assert.That(ExerciseListItems.Best(Find(Squat), new ExerciseBest(null, null, null, 40m)), Is.Null);

    [Test]
    public void Last_BestItCantShow_IsTheDateOnly() =>
        Assert.That(ExerciseListItems.Last(Find(Squat), Logged(Squat, Now.AddDays(-8), new ExerciseBest(70m, null, null, null)), TimeZoneInfo.Utc, Now),
            Is.EqualTo("Tue 20 Oct"));

    [TestCase(" ")]
    [TestCase("-")]
    [TestCase(" .' ")]
    public void Find_NoSearchWords_KeepsTheOrderGiven(string query)
    {
        ExerciseListItem[] items = [Item(Run), Item(Squat), Item(Plank)];

        Assert.That(ExerciseListItems.Find(items, Catalog, query, AddExerciseFilter.All), Is.EqualTo(items));
    }

    [Test]
    public void Find_SearchText_IsRankedAsTheCatalogsSearchAndOnlyTheItemsGiven()
    {
        ExerciseListItem[] items = [Item(Squat), Item(Run), Item(BodyweightSquat), Item(DumbbellCurl)];

        Assert.That(ExerciseListItems.Find(items, Catalog, "squat", AddExerciseFilter.All).Select(i => i.Id), Is.EqualTo(new[] { BodyweightSquat, Squat }));
    }

    [TestCase(AddExerciseFilter.Strength, new[] { Squat, Plank })]
    [TestCase(AddExerciseFilter.Cardio, new[] { Run })]
    [TestCase(AddExerciseFilter.All, new[] { Run, Squat, Plank })]
    public void Find_Filter_KeepsThatKind(AddExerciseFilter filter, int[] expected) =>
        Assert.That(ExerciseListItems.Find([Item(Run), Item(Squat), Item(Plank)], Catalog, null, filter).Select(i => i.Id), Is.EqualTo(expected));

    [Test]
    public void Find_SearchAndFilter_Combine() =>
        Assert.That(ExerciseListItems.Find(Catalog.Search(null).Select(e => Item(e.Id)).ToList(), Catalog, "row", AddExerciseFilter.Strength).Select(i => i.Id),
            Is.EqualTo(Catalog.Search("row").Where(e => e.Type == ExerciseType.Strength).Select(e => e.Id)).And.Not.Empty);

    [Test]
    public void Find_NoSearch_PutsDoneExercisesFirstMostRecentFirstThenTheRestAsGiven()
    {
        ExerciseListItem[] items = [Item(Run), Item(Squat, 5), Item(Plank), Item(DumbbellCurl, 1), Item(Run + 1), Item(BarbellCurl, 9)];

        var found = ExerciseListItems.Find(items, Catalog, null, AddExerciseFilter.All);

        Assert.Multiple(() =>
        {
            Assert.That(found.Recent.Select(i => i.Id), Is.EqualTo(new[] { DumbbellCurl, Squat, BarbellCurl }));
            Assert.That(found.Other.Select(i => i.Id), Is.EqualTo(new[] { Run, Plank, Run + 1 }));
            Assert.That(found.Select(i => i.Id), Is.EqualTo(found.Recent.Concat(found.Other).Select(i => i.Id)));
            Assert.That(found.Grouped, Is.True);
        });
    }

    [Test]
    public void Find_NoSearch_SameWorkoutTiesAreAToZ()
    {
        ExerciseListItem[] items = [Item(Run, 2), Item(Squat, 2), Item(Plank, 2)];

        Assert.That(ExerciseListItems.Find(items, Catalog, null, AddExerciseFilter.All).Recent.Select(i => i.Name),
            Is.EqualTo(new[] { "Barbell Back Squat", "Outdoor Run", "Plank" }));
    }

    [Test]
    public void Find_NothingDone_IsOneFlatListWithNoHeaders()
    {
        ExerciseListItem[] items = [Item(Run), Item(Squat), Item(Plank)];

        var found = ExerciseListItems.Find(items, Catalog, null, AddExerciseFilter.All);

        Assert.Multiple(() =>
        {
            Assert.That(found.Grouped, Is.False);
            Assert.That(found.Recent, Is.Empty);
            Assert.That(found.Other, Is.EqualTo(items));
        });
    }

    [Test]
    public void Find_Search_DoneBeatsABetterTextMatchAndEachGroupKeepsTheTextRank()
    {
        var curls = Catalog.SearchRanked("curl").Select(r => r.Exercise.Id).ToList();
        var worse = curls.Last();
        var better = curls.First();
        Assume.That(Catalog.SearchRanked("curl").First().Rank, Is.LessThan(Catalog.SearchRanked("curl").Last().Rank));
        var items = Catalog.Search(null).Select(e => e.Id == worse ? Item(e.Id, 3) : Item(e.Id)).ToList();

        var found = ExerciseListItems.Find(items, Catalog, "curl", AddExerciseFilter.All);

        Assert.Multiple(() =>
        {
            Assert.That(found.Recent.Select(i => i.Id), Is.EqualTo(new[] { worse }));
            Assert.That(found.Other.Select(i => i.Id), Is.EqualTo(curls.Where(id => id != worse)));
            Assert.That(found.Other.First().Id, Is.EqualTo(better));
        });
    }

    [Test]
    public void Find_Search_DoneGroupIsByRankThenMostRecentFirst()
    {
        var ranked = Catalog.SearchRanked("curl");
        var firstRank = ranked.First().Rank;
        var top = ranked.Where(r => r.Rank == firstRank).Select(r => r.Exercise.Id).Take(2).ToList();
        var last = ranked.Last();
        Assume.That(last.Rank, Is.GreaterThan(firstRank));
        Assume.That(top, Has.Count.EqualTo(2));
        var items = Catalog.Search(null)
            .Select(e => e.Id == last.Exercise.Id ? Item(e.Id, 1) : e.Id == top[0] ? Item(e.Id, 10) : e.Id == top[1] ? Item(e.Id, 4) : Item(e.Id))
            .ToList();

        Assert.That(ExerciseListItems.Find(items, Catalog, "curl", AddExerciseFilter.All).Recent.Select(i => i.Id),
            Is.EqualTo(new[] { top[1], top[0], last.Exercise.Id }));
    }

    [Test]
    public void Find_Search_SameRankAndSameWorkoutKeepTheCatalogOrder()
    {
        var ranked = Catalog.SearchRanked("curl");
        var sameRank = ranked.Where(r => r.Rank == ranked.First().Rank).Select(r => r.Exercise.Id).Take(2).ToList();
        Assume.That(sameRank, Has.Count.EqualTo(2));
        var items = Catalog.Search(null).Select(e => sameRank.Contains(e.Id) ? Item(e.Id, 2) : Item(e.Id)).ToList();

        Assert.That(ExerciseListItems.Find(items, Catalog, "curl", AddExerciseFilter.All).Recent.Select(i => i.Id), Is.EqualTo(sameRank));
    }

    [Test]
    public void Find_Filter_AppliesToBothGroups()
    {
        ExerciseListItem[] items = [Item(Run, 1), Item(Squat, 2), Item(Plank), Item(Run + 1)];

        var found = ExerciseListItems.Find(items, Catalog, null, AddExerciseFilter.Strength);

        Assert.Multiple(() =>
        {
            Assert.That(found.Recent.Select(i => i.Id), Is.EqualTo(new[] { Squat }));
            Assert.That(found.Other.Select(i => i.Id), Is.EqualTo(new[] { Plank }));
        });
    }

    [Test]
    public void Find_FilterHidesEveryDoneExercise_StillGroupedSoTheOtherHeaderStays()
    {
        ExerciseListItem[] items = [Item(Squat, 1), Item(Run), Item(Run + 1)];

        var found = ExerciseListItems.Find(items, Catalog, null, AddExerciseFilter.Cardio);

        Assert.Multiple(() =>
        {
            Assert.That(found.Recent, Is.Empty);
            Assert.That(found.Other, Has.Count.EqualTo(2));
            Assert.That(found.Grouped, Is.True);
        });
    }

    [Test]
    public void Find_NoMatch_IsEmpty() =>
        Assert.That(ExerciseListItems.Find([Item(Squat, 1)], Catalog, "zumba", AddExerciseFilter.All), Is.Empty);

    private static ExerciseListItem Item(int id, int daysAgo) =>
        ExerciseListItems.Item(Find(id), Logged(id, Now.AddDays(-daysAgo), null), TimeZoneInfo.Utc, Now);

    private static ExerciseDefinition Find(int id) => Catalog.All.Single(e => e.Id == id);

    private static ExerciseListItem Item(int id) => ExerciseListItems.Item(Find(id), null, TimeZoneInfo.Utc, Now);

    private static LoggedExercise Logged(int id, DateTimeOffset startedAt, ExerciseBest? best) => new(id, startedAt, best);
}
