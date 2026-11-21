using AngleSharp.Dom;
using Microsoft.Extensions.DependencyInjection;
using Shields.UI.Components.Overlays;
using Shields.UI.Theme;
using WorkoutLog.Core.Domain;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Models.Exercises;
using WorkoutLog.UI.Models.Settings;
using WorkoutLog.UI.Models.Summary;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Models.Workouts;
using WorkoutLog.UI.Navigation;
using WorkoutLog.UI.Screens;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Stories;

/// <summary>A screen state: its stories section and story name, and how to render it.</summary>
/// <param name="Section">Stories title, e.g. "Screens/New Workout".</param>
/// <param name="Name">Story name.</param>
/// <param name="Render">Renders the state with every callback set and returns its elements.</param>
public sealed record ScreenState(string Section, string Name, Func<BunitContext, IReadOnlyList<IElement>> Render)
{
    public override string ToString() => $"{Section}/{Name}";
}

/// <summary>Every screen state that has a story, in story order. Renders mirror the stories, with every callback set.</summary>
public static class ScreenStates
{
    public static IReadOnlyList<ScreenState> All { get; } =
    [
        new("Screens/This week", "Mid-week", c => ThisWeek(c, p => p
            .Add(x => x.Today, SampleThisWeek.Wednesday)
            .Add(x => x.Trained, SampleThisWeek.TrainedDays)
            .Add(x => x.Streak, 23)
            .Add(x => x.ThisWeek, SampleThisWeek.ThisWeek)
            .Add(x => x.LastWeekToDate, SampleThisWeek.LastWeekToDate)
            .Add(x => x.Workouts, SampleThisWeek.Workouts)
            .Add(x => x.Records, SampleThisWeek.Records)
            .Add(x => x.Muscles, SampleThisWeek.Muscles))),
        new("Screens/This week", "New week", c => ThisWeek(c, p => p
            .Add(x => x.Today, SampleThisWeek.Monday)
            .Add(x => x.Streak, 22)
            .Add(x => x.LastWeek, SampleThisWeek.LastWeek))),
        new("Screens/This week", "No workouts", c => ThisWeek(c, p => p.Add(x => x.Today, SampleThisWeek.Wednesday))),
        new("Screens/This week", "Records failed", c => ThisWeek(c, p => p
            .Add(x => x.Today, SampleThisWeek.Wednesday)
            .Add(x => x.Trained, SampleThisWeek.TrainedDays)
            .Add(x => x.Streak, 23)
            .Add(x => x.ThisWeek, SampleThisWeek.ThisWeek)
            .Add(x => x.LastWeekToDate, SampleThisWeek.LastWeekToDate)
            .Add(x => x.Workouts, SampleThisWeek.Workouts)
            .Add(x => x.Records, null)
            .Add(x => x.Muscles, SampleThisWeek.Muscles))),
        new("Screens/This week", "Loading", c => ThisWeek(c, p => p.Add(x => x.Loading, true))),
        new("Screens/This week", "Load failed", c => ThisWeek(c, p => p.Add(x => x.Failed, true))),

        new("Screens/Trends", "Week", c => Trends(c, SampleTrends.Week)),
        new("Screens/Trends", "Week only", c => Trends(c, SampleTrends.WeekOnly)),
        new("Screens/Trends", "Month", c => Trends(c, SampleTrends.Month)),
        new("Screens/Trends", "Year", c => Trends(c, SampleTrends.Year)),
        new("Screens/Trends", "Past week", c => Trends(c, SampleTrends.PastWeek)),
        new("Screens/Trends", "Past month", c => Trends(c, SampleTrends.PastMonth)),
        new("Screens/Trends", "Past year", c => Trends(c, SampleTrends.PastYear)),
        new("Screens/Trends", "No workouts ever", c => Trends(c, SampleTrends.NoWorkoutsEver)),
        new("Screens/Trends", "No workouts, week", c => Trends(c, SampleTrends.EmptyWeek)),
        new("Screens/Trends", "No workouts, month", c => Trends(c, SampleTrends.EmptyMonth)),
        new("Screens/Trends", "No workouts, year", c => Trends(c, SampleTrends.EmptyYear)),
        new("Screens/Trends", "Loading", c => Trends(c, SampleTrends.Loading)),
        new("Screens/Trends", "Load failed", c => Trends(c, SampleTrends.LoadFailed)),

        new("Screens/New Workout", "Empty", c => Workout(c, WorkoutMode.New, [])),
        new("Screens/New Workout", "Strength", c => Workout(c, WorkoutMode.New, SampleData.StrengthWorkoutWithWeights(Catalog(c)))),
        new("Screens/New Workout", "Done", c => Workout(c, WorkoutMode.New, SampleData.MarkDone(SampleData.StrengthWorkoutWithWeights(Catalog(c)), 1, 2))),
        new("Screens/New Workout", "Carry", c => Workout(c, WorkoutMode.New, SampleData.CarryWorkout(Catalog(c)))),
        new("Screens/New Workout", "Cardio", c => Workout(c, WorkoutMode.New, SampleData.CardioWorkout(Catalog(c)))),
        new("Screens/New Workout", "Records", c => Workout(c, WorkoutMode.New, SampleData.RecordsWorkout(Catalog(c)))),
        new("Screens/New Workout", "Last time", c => Workout(c, WorkoutMode.New, SampleData.LastTimeWorkout(Catalog(c)))),
        new("Screens/New Workout", "Last time done", c => Workout(c, WorkoutMode.New, SampleData.MarkDone(SampleData.LastTimeWorkout(Catalog(c)), 0, 3))),
        new("Screens/New Workout", "Notes", c => Workout(c, WorkoutMode.New, SampleData.WithSampleNotes(SampleData.StrengthWorkoutWithWeights(Catalog(c))), note: SampleData.Note)),
        new("Screens/New Workout", "Note over the limit", c => Workout(c, WorkoutMode.New, SampleData.WithNoteTooLong(SampleData.StrengthWorkoutWithWeights(Catalog(c))), note: SampleData.NoteTooLong, noteError: SampleData.NoteTooLongError)),
        new("Screens/New Workout", "Exercises full", c => Workout(c, WorkoutMode.New, SampleData.FullWorkout(Catalog(c)))),
        new("Screens/New Workout", "Save errors", c => Workout(c, WorkoutMode.New, SampleData.WorkoutWithErrors(Catalog(c), WorkoutChecker(c)))),
        new("Screens/New Workout", "No exercises error", c => Workout(c, WorkoutMode.New, [], error: WorkoutChecker(c).Check([]).Error)),
        new("Screens/New Workout", "Save failed", c => Workout(c, WorkoutMode.New, SampleData.StrengthWorkout(Catalog(c)), error: WorkoutChecker(c).SaveFailed)),
        new("Screens/New Workout", "Set deleted", c => Deleted(c, WorkoutMode.New, SampleData.StrengthWorkoutWithWeights(Catalog(c)), e => SampleData.DeleteSet(e, 0, 1))),
        new("Screens/New Workout", "Exercise deleted", c => Deleted(c, WorkoutMode.New, SampleData.StrengthWorkoutWithWeights(Catalog(c)), e => SampleData.DeleteExercise(e, 0))),
        new("Screens/New Workout", "Discard prompt", c => Workout(c, WorkoutMode.New, SampleData.StrengthWorkout(Catalog(c)), dialog: ("Discard this workout?", "Discard"))),

        new("Screens/Edit Workout", "Loaded", c => Workout(c, WorkoutMode.Edit, SampleData.Loaded(SampleData.StrengthWorkout(Catalog(c))))),
        new("Screens/Edit Workout", "Strength", c => Workout(c, WorkoutMode.Edit, SampleData.Opened(SampleData.StrengthWorkout(Catalog(c))))),
        new("Screens/Edit Workout", "Carry", c => Workout(c, WorkoutMode.Edit, SampleData.Opened(SampleData.CarryWorkout(Catalog(c))))),
        new("Screens/Edit Workout", "Cardio", c => Workout(c, WorkoutMode.Edit, SampleData.Opened(SampleData.CardioWorkout(Catalog(c))))),
        new("Screens/Edit Workout", "Last time", c => Workout(c, WorkoutMode.Edit, SampleData.MarkDone(SampleData.LastTimeWorkout(Catalog(c)), 1, 2, 3, 4, 6))),
        new("Screens/Edit Workout", "Notes", c => Workout(c, WorkoutMode.Edit, SampleData.Opened(SampleData.WithSampleNotes(SampleData.StrengthWorkout(Catalog(c)))), note: SampleData.Note)),
        new("Screens/Edit Workout", "Note over the limit", c => Workout(c, WorkoutMode.Edit, SampleData.WithNoteTooLong(SampleData.Opened(SampleData.StrengthWorkout(Catalog(c)))), note: SampleData.NoteTooLong, noteError: SampleData.NoteTooLongError)),
        new("Screens/Edit Workout", "Save errors", c => Workout(c, WorkoutMode.Edit, SampleData.EditWorkoutWithErrors(Catalog(c), WorkoutChecker(c)))),
        new("Screens/Edit Workout", "Save failed", c => Workout(c, WorkoutMode.Edit, SampleData.Opened(SampleData.StrengthWorkout(Catalog(c))), error: WorkoutChecker(c).SaveFailed)),
        new("Screens/Edit Workout", "Set deleted", c => Deleted(c, WorkoutMode.Edit, SampleData.Opened(SampleData.StrengthWorkout(Catalog(c))), e => SampleData.DeleteSet(e, 2, 1))),
        new("Screens/Edit Workout", "Exercise deleted", c => Deleted(c, WorkoutMode.Edit, SampleData.Loaded(SampleData.StrengthWorkout(Catalog(c))), e => SampleData.DeleteExercise(e, 0))),
        new("Screens/Edit Workout", "Discard prompt", c => Workout(c, WorkoutMode.Edit, SampleData.Opened(SampleData.StrengthWorkout(Catalog(c))), dialog: ("Discard changes?", "Discard"))),
        new("Screens/Edit Workout", "Delete prompt", c => Workout(c, WorkoutMode.Edit, SampleData.Opened(SampleData.StrengthWorkout(Catalog(c))), dialog: ("Delete this workout?", "Delete"))),
        new("Screens/Edit Workout", "Delete failed", c => Workout(c, WorkoutMode.Edit, SampleData.Opened(SampleData.StrengthWorkout(Catalog(c))), error: WorkoutChecker(c).DeleteFailed)),
        new("Screens/Edit Workout", "Loading", c => WorkoutLoad(c)),
        new("Screens/Edit Workout", "Load failed", c => WorkoutLoad(c, failed: true)),
        new("Screens/Edit Workout", "Not found", c => WorkoutLoad(c, notFound: true)),

        new("Screens/Workout Summary", "Records and changes", c => Summary(c, SampleWorkoutSummary.RecordsAndChanges)),
        new("Screens/Workout Summary", "No records", c => Summary(c, SampleWorkoutSummary.NoRecords)),
        new("Screens/Workout Summary", "First workout", c => Summary(c, SampleWorkoutSummary.FirstWorkout)),
        new("Screens/Workout Summary", "All first time", c => Summary(c, SampleWorkoutSummary.AllFirstTime)),
        new("Screens/Workout Summary", "Loading", c => Summary(c, null, loading: true)),
        new("Screens/Workout Summary", "Load failed", c => Summary(c, null, failed: true)),

        new("Screens/Add Exercise", "No search text", c => AddExercise(c, null)),
        new("Screens/Add Exercise", "Strength", c => AddExercise(c, null, AddExerciseFilter.Strength)),
        new("Screens/Add Exercise", "Cardio", c => AddExercise(c, null, AddExerciseFilter.Cardio)),
        new("Screens/Add Exercise", "Results", c => AddExercise(c, "run")),
        new("Screens/Add Exercise", "No match", c => AddExercise(c, "zumba")),
        new("Screens/Add Exercise", "Loading", c => AddExercise(c, null, loading: true)),
        new("Screens/Add Exercise", "Load failed", c => AddExercise(c, null, withLast: false)),

        new("Screens/Workouts", "Empty", c => Workouts(c, WorkoutHistory.None, default, tappable: true)),
        new("Screens/Workouts", "With workouts", c => Workouts(c, WorkoutSamples, WorkoutSamples.Span(HistoryRange.ThreeMonths), tappable: true)),
        new("Screens/Workouts", "1Y", c => Workouts(c, WorkoutSamples, WorkoutSamples.Span(HistoryRange.Year), tappable: true)),
        new("Screens/Workouts", "All", c => Workouts(c, WorkoutSamples, WorkoutSamples.Span(HistoryRange.All), tappable: true)),
        new("Screens/Workouts", "Dates picked", c => Workouts(c, WorkoutSamples, SampleWorkoutHistory.March2025, tappable: true)),
        new("Screens/Workouts", "One day", c => Workouts(c, WorkoutSamples, new DateSpan(SampleWorkoutHistory.TwoWorkoutsDay, SampleWorkoutHistory.TwoWorkoutsDay), tappable: true)),
        new("Screens/Workouts", "Nothing in range", c => Workouts(c, WorkoutSamples, SampleWorkoutHistory.NoWorkouts, tappable: true)),
        new("Screens/Workouts", "No workout on this day", c => Workouts(c, WorkoutSamples, SampleWorkoutHistory.RestDay, tappable: true)),
        new("Screens/Workouts", "Picking dates", c => Workouts(c, WorkoutSamples, WorkoutSamples.Span(HistoryRange.ThreeMonths), tappable: true, expanded: true)),
        new("Screens/Workouts", "Picking month", c => Workouts(c, WorkoutSamples, WorkoutSamples.Span(HistoryRange.ThreeMonths), tappable: true, expanded: true, months: true)),
        new("Screens/Workouts", "Loading", c => Workouts(c, WorkoutHistory.None, default, tappable: false, loading: true)),
        new("Screens/Workouts", "Load failed", c => Workouts(c, WorkoutHistory.None, default, tappable: false, failed: true)),

        new("Screens/Settings", "With workouts", c => Settings(c, 42)),
        new("Screens/Settings", "Dark chosen", c => Settings(c, 42, theme: ThemePreference.Dark)),
        new("Screens/Settings", "One workout", c => Settings(c, 1)),
        new("Screens/Settings", "No workouts", c => Settings(c, 0)),
        new("Screens/Settings", "Loading", c => Settings(c, 0, loading: true)),
        new("Screens/Settings", "Load failed", c => Settings(c, 0, failed: true)),
        new("Screens/Settings", "Exporting", c => Settings(c, 42, exporting: true)),
        new("Screens/Settings", "Export failed", c => Settings(c, 42, message: ExportMessage.Failed)),
        new("Screens/Settings", "Too many workouts", c => Settings(c, 12345, message: ExportMessage.TooLarge)),
        new("Screens/Settings", "Reading", c => Settings(c, 42, reading: true)),
        new("Screens/Settings", "Import failed", c => Settings(c, 42, importFailed: true)),
        new("Screens/Settings", "Imported", c => Settings(c, 45, imported: new Imported(3, 2, 1))),

        new("Screens/Import Review", "New only", c => ReviewScreen(c, Review(fresh: 44))),
        new("Screens/Import Review", "With all groups", c => ReviewScreen(c, Review(3, 38, 2, 1))),
        new("Screens/Import Review", "Different", c => ReviewScreen(c, Review(already: 40, different: 2), different: DifferentChoice.UseFiles)),
        new("Screens/Import Review", "Can't be imported", c => ReviewScreen(c, Review(fresh: 40, skipped: 4))),
        new("Screens/Import Review", "Nothing new", c => ReviewScreen(c, Review(already: 41))),
        new("Screens/Import Review", "Replace all", c => ReviewScreen(c, Review(3, 38, 2, 1), ImportMode.ReplaceAll)),
        new("Screens/Import Review", "Replace confirm", c => ReviewScreen(c, Review(3, 38, 2, 1), ImportMode.ReplaceAll, confirming: true)),
        new("Screens/Import Review", "Importing", c => ReviewScreen(c, Review(3, 38, 2, 1), importing: true)),
        new("Screens/Import Review", "Import failed", c => ReviewScreen(c, Review(3, 38, 2, 1), failed: true)),
        new("Screens/Import Review", "Not an export", c => ReviewScreen(c, ImportReview.Rejected("photo.json", ImportError.NotAnExport))),
        new("Screens/Import Review", "Newer version", c => ReviewScreen(c, ImportReview.Rejected("f.json", ImportError.NewerVersion))),
        new("Screens/Import Review", "No workouts", c => ReviewScreen(c, ImportReview.Rejected("f.json", ImportError.NoWorkouts))),
        new("Screens/Import Review", "Too large", c => ReviewScreen(c, ImportReview.Rejected("f.json", ImportError.TooLarge))),
        new("Screens/Import Review", "Reading another file", c => ReviewScreen(c, ImportReview.Rejected("photo.json", ImportError.NotAnExport), reading: true)),

        new("Screens/Exercise", "Lift top set", c => ExercisePage(c, SampleExercises.LiftTopSet)),
        new("Screens/Exercise", "Lift e1RM", c => ExercisePage(c, SampleExercises.LiftE1rm)),
        new("Screens/Exercise", "Lift e1RM no data", c => ExercisePage(c, SampleExercises.LiftE1rmNoData)),
        new("Screens/Exercise", "Bodyweight best set", c => ExercisePage(c, SampleExercises.BodyweightBestSet)),
        new("Screens/Exercise", "Bodyweight total", c => ExercisePage(c, SampleExercises.BodyweightTotal)),
        new("Screens/Exercise", "Bodyweight top set never weighted", c => ExercisePage(c, SampleExercises.BodyweightTopSetNeverWeighted)),
        new("Screens/Exercise", "Loaded bodyweight", c => ExercisePage(c, SampleExercises.LoadedBodyweight)),
        new("Screens/Exercise", "Assisted", c => ExercisePage(c, SampleExercises.Assisted)),
        new("Screens/Exercise", "Hold duration", c => ExercisePage(c, SampleExercises.HoldDuration)),
        new("Screens/Exercise", "Hold weight", c => ExercisePage(c, SampleExercises.HoldWeight)),
        new("Screens/Exercise", "Hold weight no data", c => ExercisePage(c, SampleExercises.HoldWeightNoData)),
        new("Screens/Exercise", "Carry weight", c => ExercisePage(c, SampleExercises.CarryWeight)),
        new("Screens/Exercise", "Carry distance", c => ExercisePage(c, SampleExercises.CarryDistance)),
        new("Screens/Exercise", "Carry weight no data", c => ExercisePage(c, SampleExercises.CarryWeightNoData)),
        new("Screens/Exercise", "Cardio distance", c => ExercisePage(c, SampleExercises.CardioDistance)),
        new("Screens/Exercise", "Cardio duration", c => ExercisePage(c, SampleExercises.CardioDuration)),
        new("Screens/Exercise", "Cardio pace", c => ExercisePage(c, SampleExercises.CardioPace)),
        new("Screens/Exercise", "Cardio speed", c => ExercisePage(c, SampleExercises.CardioSpeed)),
        new("Screens/Exercise", "Cardio no data", c => ExercisePage(c, SampleExercises.CardioNoData)),
        new("Screens/Exercise", "Cardio floors", c => ExercisePage(c, SampleExercises.CardioFloors)),
        new("Screens/Exercise", "No sessions in range", c => ExercisePage(c, SampleExercises.NoSessionsInRange)),
        new("Screens/Exercise", "One session", c => ExercisePage(c, SampleExercises.OneSession)),
        new("Screens/Exercise", "Loading", c => ExerciseLoad(c, loading: true)),
        new("Screens/Exercise", "Load failed", c => ExerciseLoad(c, failed: true)),
    ];

    /// <summary>Adds what an Exercise sample shows: every <c>ExerciseView</c> parameter except range, state and callbacks.</summary>
    public static void ExerciseParameters(ComponentParameterCollectionBuilder<ExerciseView> parameters, SampleExercise sample) =>
        parameters
            .Add(x => x.Name, sample.Name)
            .Add(x => x.Subtitle, sample.Subtitle)
            .Add(x => x.Tabs, sample.Tabs)
            .Add(x => x.Tab, sample.Tab)
            .Add(x => x.ChartTitle, sample.ChartTitle)
            .Add(x => x.Points, sample.Points)
            .Add(x => x.Flipped, sample.Flipped)
            .Add(x => x.Format, sample.Format)
            .Add(x => x.TimeScale, sample.TimeScale)
            .Add(x => x.Caption, sample.Caption)
            .Add(x => x.NoDataMessage, sample.NoDataMessage)
            .Add(x => x.Records, sample.Records)
            .Add(x => x.Sessions, sample.Sessions);

    private static IExerciseCatalog Catalog(BunitContext context) => context.Services.GetRequiredService<IExerciseCatalog>();

    private static ISetChecker Checker(BunitContext context) => context.Services.GetRequiredService<ISetChecker>();

    private static IWorkoutChecker WorkoutChecker(BunitContext context) => context.Services.GetRequiredService<IWorkoutChecker>();

    private static IReadOnlyList<IElement> ThisWeek(BunitContext context, Action<ComponentParameterCollectionBuilder<ThisWeekView>> state) =>
        context.Render<ThisWeekView>(p =>
        {
            p.Add(x => x.Monday, SampleThisWeek.Monday)
                .Add(x => x.OnRetry, () => { })
                .Add(x => x.OnSettings, () => { })
                .Add(x => x.OnNew, () => { })
                .Add(x => x.OnNavigate, (MainTab _) => { })
                .Add(x => x.OnOpenWorkout, (int _) => { })
                .Add(x => x.OnOpenRecord, (int _) => { })
                .Add(x => x.OnOpenLastWeek, () => { });
            state(p);
        }).FindAll("*");

    private static IReadOnlyList<IElement> Trends(BunitContext context, IReadOnlyDictionary<string, object?> state) =>
        context.Render<TrendsView>(p => p.Sample(state)
            .Add(x => x.OnKindChanged, (PeriodKind _) => { })
            .Add(x => x.OnPrevious, () => { })
            .Add(x => x.OnNext, () => { })
            .Add(x => x.OnRetry, () => { })
            .Add(x => x.OnSettings, () => { })
            .Add(x => x.OnNew, () => { })
            .Add(x => x.OnNavigate, (MainTab _) => { })
            .Add(x => x.OnOpenRecord, (int _) => { })
            .Add(x => x.OnOpenWorkouts, (DateSpan _) => { })).FindAll("*");

    /// <summary>Sets each parameter of a <see cref="SampleTrends"/> state on the view.</summary>
    public static ComponentParameterCollectionBuilder<TrendsView> Sample(this ComponentParameterCollectionBuilder<TrendsView> parameters, IReadOnlyDictionary<string, object?> state)
    {
        foreach (var (name, value) in state)
        {
            parameters.TryAdd(name, value);
        }

        return parameters;
    }
    private static IReadOnlyList<IElement> Workout(
        BunitContext context,
        WorkoutMode mode,
        List<ExerciseState> exercises,
        string? error = null,
        (string Title, string ConfirmText)? dialog = null,
        string? note = null,
        string? noteError = null,
        PendingUndo? undo = null) =>
        context.Render<WorkoutView>(p =>
        {
            p.Add(x => x.Mode, mode)
                .Add(x => x.StartedAt, SampleData.Started)
                .Add(x => x.Time, new TimeSpan(0, 34, 12))
                .Add(x => x.Exercises, exercises)
                .Add(x => x.Error, error)
                .Add(x => x.Note, note)
                .Add(x => x.NoteError, noteError)
                .Add(x => x.NoteChanged, (string? _) => { })
                .Add(x => x.ValidateSet, Checker(context).Check)
                .Add(x => x.OnAddExercise, () => { })
                .Add(x => x.OnRemoveExercise, (ExerciseState _) => { })
                .Add(x => x.OnSave, () => { })
                .Add(x => x.OnBack, () => { });
            if (mode == WorkoutMode.Edit)
            {
                p.Add(x => x.OnDelete, () => { });
            }

            if (undo is not null)
            {
                p.Add(x => x.Undo, undo)
                    .Add(x => x.OnUndo, () => { })
                    .Add(x => x.OnUndoTimeout, (PendingUndo _) => { });
            }

            if (dialog is { } d)
            {
                p.Add<ConfirmDialog>(x => x.Overlay, o => o
                    .Add(x => x.Title, d.Title)
                    .Add(x => x.ConfirmText, d.ConfirmText)
                    .Add(x => x.Danger, true)
                    .Add(x => x.OnConfirm, () => { })
                    .Add(x => x.OnCancel, () => { }));
            }
        }).FindAll("*");

    /// <summary>The workout after <paramref name="delete"/>, with its snackbar.</summary>
    private static IReadOnlyList<IElement> Deleted(BunitContext context, WorkoutMode mode, List<ExerciseState> exercises, Func<List<ExerciseState>, PendingUndo> delete)
    {
        var undo = delete(exercises);
        return Workout(context, mode, exercises, undo: undo);
    }

    private static IReadOnlyList<IElement> WorkoutLoad(BunitContext context, bool failed = false, bool notFound = false) =>
        context.Render<WorkoutLoadView>(p => p
            .Add(x => x.Failed, failed)
            .Add(x => x.NotFound, notFound)
            .Add(x => x.OnRetry, () => { })
            .Add(x => x.OnBack, () => { }))
            .FindAll("*");

    private static IReadOnlyList<IElement> Summary(BunitContext context, WorkoutSummary? summary, bool loading = false, bool failed = false) =>
        context.Render<WorkoutSummaryView>(p => p
            .Add(x => x.Summary, summary)
            .Add(x => x.Loading, loading)
            .Add(x => x.Failed, failed)
            .Add(x => x.OnRetry, () => { })
            .Add(x => x.OnDone, () => { }))
            .FindAll("*");

    private static WorkoutHistory WorkoutSamples => SampleWorkoutHistory.History;

    private static IReadOnlyList<IElement> Workouts(
        BunitContext context, WorkoutHistory history, DateSpan dates, bool tappable, bool loading = false, bool failed = false,
        bool expanded = false, bool months = false) =>
        context.Render<WorkoutsView>(p =>
        {
            p.Add(x => x.History, history)
                .Add(x => x.Rows, history.IsEmpty ? [] : SampleWorkoutHistory.In(dates))
                .Add(x => x.Dates, dates)
                .Add(x => x.DatesChanged, (DateSpan _) => { })
                .Add(x => x.Expanded, expanded)
                .Add(x => x.ExpandedChanged, (bool _) => { })
                .Add(x => x.OpenOnMonths, months)
                .Add(x => x.Loading, loading)
                .Add(x => x.Failed, failed)
                .Add(x => x.OnRetry, () => { })
                .Add(x => x.OnBack, () => { });
            if (tappable)
            {
                p.Add(x => x.OnOpen, (WorkoutsRow _) => { });
            }
        }).FindAll("*");

    private static IReadOnlyList<IElement> Settings(
        BunitContext context,
        int count,
        bool loading = false,
        bool failed = false,
        bool exporting = false,
        ExportMessage message = ExportMessage.None,
        bool reading = false,
        bool importFailed = false,
        Imported? imported = null,
        ThemePreference theme = ThemePreference.System) =>
        context.Render<SettingsView>(p => p
            .Add(x => x.Theme, theme)
            .Add(x => x.Count, count)
            .Add(x => x.Loading, loading)
            .Add(x => x.Failed, failed)
            .Add(x => x.Exporting, exporting)
            .Add(x => x.ExportMessage, message)
            .Add(x => x.Reading, reading)
            .Add(x => x.ImportFailed, importFailed)
            .Add(x => x.Imported, imported)
            .Add(x => x.OnRetry, () => { })
            .Add(x => x.OnExport, () => { })
            .Add(x => x.OnImport, () => { })
            .Add(x => x.OnBack, () => { }))
            .FindAll("*");

    private static IReadOnlyList<IElement> AddExercise(
        BunitContext context, string? query, AddExerciseFilter filter = AddExerciseFilter.All, bool loading = false, bool withLast = true) =>
        context.Render<AddExerciseView>(p => p
            .Add(x => x.Items, loading ? [] : SampleData.AddExerciseItems(Catalog(context), query, filter, withLast))
            .Add(x => x.Query, query)
            .Add(x => x.Filter, filter)
            .Add(x => x.Loading, loading)
            .Add(x => x.QueryChanged, (string _) => { })
            .Add(x => x.FilterChanged, (AddExerciseFilter _) => { })
            .Add(x => x.OnPick, (ExerciseListItem _) => { })
            .Add(x => x.OnClose, () => { }))
            .FindAll("*");

    private static IReadOnlyList<IElement> ExercisePage(BunitContext context, SampleExercise sample) =>
        context.Render<ExerciseView>(p =>
        {
            ExerciseParameters(p, sample);
            ExerciseCallbacks(p);
        }).FindAll("*");

    private static IReadOnlyList<IElement> ExerciseLoad(BunitContext context, bool loading = false, bool failed = false) =>
        context.Render<ExerciseView>(p =>
        {
            p.Add(x => x.Name, "Barbell Back Squat")
                .Add(x => x.Subtitle, "Quadriceps · Barbell")
                .Add(x => x.Loading, loading)
                .Add(x => x.Failed, failed);
            ExerciseCallbacks(p);
        }).FindAll("*");

    private static void ExerciseCallbacks(ComponentParameterCollectionBuilder<ExerciseView> parameters) =>
        parameters
            .Add(x => x.TabChanged, (string _) => { })
            .Add(x => x.RangeChanged, (HistoryRange _) => { })
            .Add(x => x.OnRetry, () => { })
            .Add(x => x.OnBack, () => { });

    /// <summary>A review of a file against 41 workouts on the phone.</summary>
    private static ImportReview Review(int fresh = 0, int already = 0, int different = 0, int skipped = 0) =>
        new("workout-log-2026-10-26-1805.json", fresh + already + different + skipped, fresh, already, different, skipped, PhoneCount: 41);

    private static IReadOnlyList<IElement> ReviewScreen(
        BunitContext context,
        ImportReview review,
        ImportMode mode = ImportMode.Add,
        DifferentChoice different = DifferentChoice.KeepPhones,
        bool confirming = false,
        bool importing = false,
        bool failed = false,
        bool reading = false) =>
        context.Render<ImportReviewView>(p => p
            .Add(x => x.Review, review)
            .Add(x => x.Mode, mode)
            .Add(x => x.Different, different)
            .Add(x => x.Confirming, confirming)
            .Add(x => x.Importing, importing)
            .Add(x => x.Failed, failed)
            .Add(x => x.Reading, reading)
            .Add(x => x.ModeChanged, (ImportMode _) => { })
            .Add(x => x.DifferentChanged, (DifferentChoice _) => { })
            .Add(x => x.OnImport, () => { })
            .Add(x => x.OnConfirmReplace, () => { })
            .Add(x => x.OnCancelReplace, () => { })
            .Add(x => x.OnCancel, () => { })
            .Add(x => x.OnChooseAnother, () => { })
            .Add(x => x.OnBack, () => { }))
            .FindAll("*");
}
