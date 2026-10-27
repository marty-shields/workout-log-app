using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Import;
using WorkoutLog.Core.Import.Upgrades;
using WorkoutLog.Core.Services;
using static WorkoutLog.Core.Tests.Import.ImportJson;

namespace WorkoutLog.Core.Tests.Import;

public class ImportFileReaderTests
{
    private readonly ImportFileReader reader = new([]);

    [Test]
    public async Task ReadAsync_Export_ReturnsEachWorkoutInFileOrder()
    {
        var read = await ReadAsync(File(Workout(2, Bench, Run), Workout(0)));

        Assert.That(read.Error, Is.EqualTo(ImportError.None));
        Assert.That(read.Exception, Is.Null);
        Assert.That(read.Workouts, Is.EqualTo(new[]
        {
            new ImportWorkout(1, Time(Start.AddHours(2)), Time(Start.AddHours(3)),
            [
                new ImportExercise(new ImportExerciseInfo(8, "Barbell Bench Press", ExerciseType.Strength), [new ImportSet(8, 60m)]),
                new ImportExercise(new ImportExerciseInfo(311, "Outdoor Run", ExerciseType.Cardio, DistanceUnit.Km, PaceUnit: PaceUnit.PerKm),
                    Entry: new ImportEntry(TimeSpan.FromMinutes(30), 5.2m, Pace: new TimeSpan(0, 5, 46))),
            ]),
            new ImportWorkout(2, Time(Start), Time(Start.AddHours(1)), []),
        }).UsingPropertiesComparer());
    }

    [Test]
    public async Task ReadAsync_EveryMetric_IsRead()
    {
        var read = await ReadAsync(File(Workout(0,
            """{"exercise":{"id":187},"sets":[{"reps":1,"weightKg":32.5,"duration":"100:00:45","distance":20}]}""",
            """{"exercise":{"id":300},"entry":{"duration":"01:02:03","distance":10,"floors":4,"level":5,"incline":1.5,"calories":310,"rate":9.5,"pace":"00:06:00"}}""")));

        Assert.That(read.Workouts.Single().Exercises, Is.EqualTo(new[]
        {
            new ImportExercise(new ImportExerciseInfo(187), [new ImportSet(1, 32.5m, new TimeSpan(100, 0, 45), 20m)]),
            new ImportExercise(new ImportExerciseInfo(300), Entry: new ImportEntry(new TimeSpan(1, 2, 3), 10m, 4, 5, 1.5m, 310, 9.5m, TimeSpan.FromMinutes(6))),
        }).UsingPropertiesComparer());
    }

    [Test]
    public async Task ReadAsync_ByteOrderMark_IsIgnored()
    {
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(File(Workout(0)))).ToArray();

        var read = await reader.ReadAsync(new MemoryStream(bytes));

        Assert.That(read.Error, Is.EqualTo(ImportError.None));
    }

    [Test]
    public async Task ReadAsync_ExactlyTheLimit_IsRead()
    {
        var read = await ReadAsync(PaddedTo(5_242_880));

        Assert.That(read.Error, Is.EqualTo(ImportError.None));
    }

    [Test]
    public async Task ReadAsync_OneByteOverTheLimit_IsTooLarge()
    {
        var read = await ReadAsync(PaddedTo(5_242_881));

        Assert.That(read, Is.EqualTo(new ImportFileRead(ImportError.TooLarge, [])).UsingPropertiesComparer());
    }

    [Test]
    public async Task ReadAsync_NotJson_IsNotAnExportWithTheException()
    {
        var read = await ReadAsync("workouts, but not JSON");

        Assert.That(read.Error, Is.EqualTo(ImportError.NotAnExport));
        Assert.That(read.Workouts, Is.Empty);
        Assert.That(read.Exception, Is.InstanceOf<JsonException>());
    }

    [TestCase("")]
    [TestCase("null")]
    [TestCase("[]")]
    [TestCase("\"text\"")]
    [TestCase("""{"formatVersion":1,"formatVersion":1,"exportedAt":"2026-10-26T18:05:12Z","appVersion":"1.4.0","workouts":[{}]}""")]
    public async Task ReadAsync_NotAnObjectOrInvalid_IsNotAnExport(string json)
    {
        Assert.That((await ReadAsync(json)).Error, Is.EqualTo(ImportError.NotAnExport));
    }

    [TestCase("""{"exportedAt":"2026-10-26T18:05:12Z","appVersion":"1.4.0","workouts":[{}]}""")]
    [TestCase("""{"formatVersion":"1","exportedAt":"2026-10-26T18:05:12Z","appVersion":"1.4.0","workouts":[{}]}""")]
    [TestCase("""{"formatVersion":1.5,"exportedAt":"2026-10-26T18:05:12Z","appVersion":"1.4.0","workouts":[{}]}""")]
    [TestCase("""{"formatVersion":0,"exportedAt":"2026-10-26T18:05:12Z","appVersion":"1.4.0","workouts":[{}]}""")]
    [TestCase("""{"formatVersion":-1,"exportedAt":"2026-10-26T18:05:12Z","appVersion":"1.4.0","workouts":[{}]}""")]
    [TestCase("""{"formatVersion":null,"exportedAt":"2026-10-26T18:05:12Z","appVersion":"1.4.0","workouts":[{}]}""")]
    [TestCase("""{"formatVersion":1,"appVersion":"1.4.0","workouts":[{}]}""")]
    [TestCase("""{"formatVersion":1,"exportedAt":"2026-10-26","appVersion":"1.4.0","workouts":[{}]}""")]
    [TestCase("""{"formatVersion":1,"exportedAt":"2026-10-26T18:05:12.1234567Z","appVersion":"1.4.0","workouts":[{}]}""")]
    [TestCase("""{"formatVersion":1,"exportedAt":"2026-13-26T18:05:12Z","appVersion":"1.4.0","workouts":[{}]}""")]
    [TestCase("""{"formatVersion":1,"exportedAt":20261026,"appVersion":"1.4.0","workouts":[{}]}""")]
    [TestCase("""{"formatVersion":1,"exportedAt":"2026-10-26T18:05:12Z","workouts":[{}]}""")]
    [TestCase("""{"formatVersion":1,"exportedAt":"2026-10-26T18:05:12Z","appVersion":" ","workouts":[{}]}""")]
    [TestCase("""{"formatVersion":1,"exportedAt":"2026-10-26T18:05:12Z","appVersion":140,"workouts":[{}]}""")]
    [TestCase("""{"formatVersion":1,"exportedAt":"2026-10-26T18:05:12Z","appVersion":"1.4.0"}""")]
    [TestCase("""{"formatVersion":1,"exportedAt":"2026-10-26T18:05:12Z","appVersion":"1.4.0","workouts":{}}""")]
    [TestCase("""{"formatVersion":1,"exportedAt":"2026-10-26T18:05:12Z","appVersion":"1.4.0","workouts":null}""")]
    public async Task ReadAsync_HeaderMissingOrInvalid_IsNotAnExport(string json)
    {
        var read = await ReadAsync(json);

        Assert.That(read, Is.EqualTo(new ImportFileRead(ImportError.NotAnExport, [])).UsingPropertiesComparer());
    }

    [Test]
    public async Task ReadAsync_NewerFormatVersion_IsNewerVersionWhateverElseItHolds()
    {
        Assert.That((await ReadAsync("""{"formatVersion":2,"somethingNew":true}""")).Error, Is.EqualTo(ImportError.NewerVersion));
    }

    [Test]
    public async Task ReadAsync_NoWorkouts_IsNoWorkouts()
    {
        Assert.That((await ReadAsync(File())).Error, Is.EqualTo(ImportError.NoWorkouts));
    }

    [Test]
    public async Task ReadAsync_UnknownProperties_AreIgnored()
    {
        var json = File(Workout(0, """{"exercise":{"id":8,"extra":1},"sets":[{"reps":8,"note":"x"}],"more":[]}""")).Replace("\"appVersion\"", "\"other\":{},\"appVersion\"");

        var read = await ReadAsync(json);

        Assert.That(read.Workouts.Single().Exercises,
            Is.EqualTo(new[] { new ImportExercise(new ImportExerciseInfo(8), [new ImportSet(8)]) }).UsingPropertiesComparer());
    }

    [TestCase("""{"startedAt":"S","finishedAt":"F"}""", TestName = "No exercises")]
    [TestCase("""{"startedAt":"S","finishedAt":"F","exercises":null}""", TestName = "Null exercises")]
    [TestCase("""{"startedAt":"S","finishedAt":"F","exercises":{}}""", TestName = "Exercises not a list")]
    [TestCase("""{"startedAt":"S","finishedAt":"F","exercises":[null]}""", TestName = "Null exercise")]
    [TestCase("""{"startedAt":"S","finishedAt":"F","exercises":[{"sets":[]}]}""", TestName = "No exercise info")]
    [TestCase("""{"startedAt":"S","finishedAt":"F","exercises":[{"exercise":{},"sets":[]}]}""", TestName = "No exercise id")]
    [TestCase("""{"startedAt":"S","finishedAt":"F","exercises":[{"exercise":{"id":"8"},"sets":[]}]}""", TestName = "Id as text")]
    [TestCase("""{"startedAt":"S","finishedAt":"F","exercises":[{"exercise":{"id":8}}]}""", TestName = "Neither sets nor entry")]
    [TestCase("""{"startedAt":"S","finishedAt":"F","exercises":[{"exercise":{"id":8},"sets":[],"entry":{}}]}""", TestName = "Both sets and entry")]
    [TestCase("""{"startedAt":"S","finishedAt":"F","exercises":[{"exercise":{"id":8},"sets":[null]}]}""", TestName = "Null set")]
    [TestCase("""{"startedAt":"S","finishedAt":"F","exercises":[{"exercise":{"id":8},"sets":[{"weightKg":"60"}]}]}""", TestName = "Decimal as text")]
    [TestCase("""{"startedAt":"S","finishedAt":"F","exercises":[{"exercise":{"id":8},"sets":[{"reps":5.5}]}]}""", TestName = "Fractional reps")]
    [TestCase("""{"startedAt":"S","finishedAt":"F","exercises":[{"exercise":{"id":8},"sets":[{"weightKg":1e40}]}]}""", TestName = "Decimal out of range")]
    [TestCase("""{"startedAt":"S","finishedAt":"F","exercises":[{"exercise":{"id":8},"sets":[{"duration":"0:01:00"}]}]}""", TestName = "Short hours")]
    [TestCase("""{"startedAt":"S","finishedAt":"F","exercises":[{"exercise":{"id":8},"sets":[{"duration":"00:60:00"}]}]}""", TestName = "Minutes over 59")]
    [TestCase("""{"startedAt":"S","finishedAt":"F","exercises":[{"exercise":{"id":8},"sets":[{"duration":"00:00:60"}]}]}""", TestName = "Seconds over 59")]
    [TestCase("""{"startedAt":"S","finishedAt":"F","exercises":[{"exercise":{"id":8},"sets":[{"duration":60}]}]}""", TestName = "Duration as a number")]
    [TestCase("""{"startedAt":"S","finishedAt":"F","exercises":[{"exercise":{"id":8},"sets":[{"duration":"99999999999:00:00"}]}]}""", TestName = "Hours out of range")]
    [TestCase("""{"startedAt":"S","finishedAt":"F","exercises":[{"exercise":{"id":8,"type":0},"sets":[]}]}""", TestName = "Type as a number")]
    [TestCase("""{"startedAt":"S","finishedAt":"F","exercises":[{"exercise":{"id":8,"type":"Yoga"},"sets":[]}]}""", TestName = "Unknown type")]
    [TestCase("""{"startedAt":"S","finishedAt":"F","exercises":[{"exercise":{"id":8,"Id":9},"Sets":[]}]}""", TestName = "Wrong case")]
    public async Task ReadAsync_WorkoutThatCantBeRead_IsMalformedWithItsTimeTexts(string workout)
    {
        var read = await ReadAsync(File(Workout(0), workout));

        Assert.That(read.Error, Is.EqualTo(ImportError.None));
        Assert.That(read.Workouts[1], Is.EqualTo(new ImportWorkout(2, "S", "F", null)).UsingPropertiesComparer());
    }

    [TestCase("null")]
    [TestCase("[]")]
    [TestCase("1")]
    public async Task ReadAsync_WorkoutNotAnObject_IsMalformedWithNoTimes(string workout)
    {
        var read = await ReadAsync(File(workout));

        Assert.That(read.Workouts, Is.EqualTo(new[] { new ImportWorkout(1, null, null, null) }).UsingPropertiesComparer());
    }

    [Test]
    public async Task ReadAsync_TimesThatArentText_AreNull()
    {
        var read = await ReadAsync(File("""{"startedAt":1,"finishedAt":null,"exercises":[]}"""));

        Assert.That(read.Workouts, Is.EqualTo(new[] { new ImportWorkout(1, null, null, []) }).UsingPropertiesComparer());
    }

    [Test]
    public void ReadAsync_StreamThrows_Throws()
    {
        var stream = new ThrowingStream();

        Assert.That(() => reader.ReadAsync(stream), Throws.TypeOf<IOException>());
    }

    [Test]
    public async Task ReadAsync_OlderVersion_AppliesTheStepsAfterItInOrderBeforeReading()
    {
        var applied = new List<int>();
        var upgrades = new[] { new RecordingUpgrade(3, applied), new RecordingUpgrade(2, applied), new RecordingUpgrade(4, applied) };
        var json = File(Workout(0)).Replace("\"formatVersion\":1", "\"formatVersion\":2").Replace("\"workouts\"", "\"sessions\"");

        var read = await new ImportFileReader(upgrades, currentVersion: 4, maxBytes: 1000).ReadAsync(Stream(json));

        Assert.That(applied, Is.EqualTo(new[] { 3, 4 }));
        Assert.That(read.Error, Is.EqualTo(ImportError.None));
        Assert.That(read.Workouts, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task ReadAsync_CurrentVersion_AppliesNoSteps()
    {
        var applied = new List<int>();

        await new ImportFileReader([new RecordingUpgrade(2, applied)], currentVersion: 2, maxBytes: 1000)
            .ReadAsync(Stream(File(Workout(0)).Replace("\"formatVersion\":1", "\"formatVersion\":2")));

        Assert.That(applied, Is.Empty);
    }

    [TestCase(1, new int[0])]
    [TestCase(3, new[] { 2, 3 })]
    public void Constructor_OneStepPerVersionFrom2_DoesNotThrow(int currentVersion, int[] versions)
    {
        Assert.That(() => new ImportFileReader(versions.Select(v => new RecordingUpgrade(v, [])), currentVersion, 1000), Throws.Nothing);
    }

    [TestCase(1, new[] { 2 })]
    [TestCase(3, new[] { 2 })]
    [TestCase(3, new[] { 2, 2, 3 })]
    [TestCase(3, new[] { 3, 4 })]
    public void Constructor_StepsNotOnePerVersionFrom2_Throws(int currentVersion, int[] versions)
    {
        Assert.That(() => new ImportFileReader(versions.Select(v => new RecordingUpgrade(v, [])), currentVersion, 1000),
            Throws.InvalidOperationException);
    }

    private Task<ImportFileRead> ReadAsync(string json) => reader.ReadAsync(Stream(json));

    /// <summary>A file of one workout, its app version padded so the file is <paramref name="bytes"/> long.</summary>
    private static string PaddedTo(int bytes)
    {
        var file = File(Workout(0));
        return file.Replace("1.4.0", new string('9', bytes - Encoding.UTF8.GetByteCount(file) + "1.4.0".Length));
    }

    /// <summary>Records its version when applied; renames <c>sessions</c> to <c>workouts</c> if present.</summary>
    private sealed class RecordingUpgrade(int version, List<int> applied) : IFormatUpgrade
    {
        public int Version => version;

        public void Apply(JsonObject file)
        {
            applied.Add(version);
            if (file.Remove("sessions", out var sessions))
            {
                file["workouts"] = sessions;
            }
        }
    }

    private sealed class ThrowingStream : MemoryStream
    {
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            throw new IOException("unreadable");
    }
}
