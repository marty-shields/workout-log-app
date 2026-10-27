using AngleSharp.Dom;
using Microsoft.Extensions.DependencyInjection;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Components.Overlays;
using WorkoutLog.UI.Components.Workout;
using WorkoutLog.UI.Models.Dashboard;
using WorkoutLog.UI.Models.History;
using WorkoutLog.UI.Models.Workout;
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
        new("Screens/Startup", "Loading", c => c.Render<StartupView>().FindAll("*")),
        new("Screens/Startup", "Load failed", c => c.Render<StartupView>(p => p.Add(x => x.Failed, true)).FindAll("*")),

        new("Screens/Dashboard", "Empty", c => Dashboard(c, new DashboardSummary(null, 0, 0))),
        new("Screens/Dashboard", "With workouts", c => Dashboard(c, new DashboardSummary(SampleData.Started, 2, 9))),
        new("Screens/Dashboard", "Loading", c => Dashboard(c, new DashboardSummary(null, 0, 0), loading: true)),
        new("Screens/Dashboard", "Load failed", c => Dashboard(c, new DashboardSummary(null, 0, 0), failed: true)),

        new("Screens/New Workout", "Empty", c => Workout(c, WorkoutMode.New, [])),
        new("Screens/New Workout", "Strength", c => Workout(c, WorkoutMode.New, SampleData.StrengthWorkoutWithWeights(Catalog(c)))),
        new("Screens/New Workout", "Carry", c => Workout(c, WorkoutMode.New, SampleData.CarryWorkout(Catalog(c)))),
        new("Screens/New Workout", "Cardio", c => Workout(c, WorkoutMode.New, SampleData.CardioWorkout(Catalog(c)))),
        new("Screens/New Workout", "Save errors", c => Workout(c, WorkoutMode.New, SampleData.WorkoutWithErrors(Catalog(c), WorkoutChecker(c)))),
        new("Screens/New Workout", "No exercises error", c => Workout(c, WorkoutMode.New, [], error: WorkoutChecker(c).Check([]).Error)),
        new("Screens/New Workout", "Save failed", c => Workout(c, WorkoutMode.New, SampleData.StrengthWorkout(Catalog(c)), error: WorkoutChecker(c).SaveFailed)),
        new("Screens/New Workout", "Discard prompt", c => Workout(c, WorkoutMode.New, SampleData.StrengthWorkout(Catalog(c)), dialog: ("Discard this workout?", "Discard"))),

        new("Screens/Edit Workout", "Strength", c => Workout(c, WorkoutMode.Edit, SampleData.Opened(SampleData.StrengthWorkout(Catalog(c))))),
        new("Screens/Edit Workout", "Carry", c => Workout(c, WorkoutMode.Edit, SampleData.Opened(SampleData.CarryWorkout(Catalog(c))))),
        new("Screens/Edit Workout", "Cardio", c => Workout(c, WorkoutMode.Edit, SampleData.Opened(SampleData.CardioWorkout(Catalog(c))))),
        new("Screens/Edit Workout", "Save errors", c => Workout(c, WorkoutMode.Edit, SampleData.EditWorkoutWithErrors(Catalog(c), WorkoutChecker(c)))),
        new("Screens/Edit Workout", "Save failed", c => Workout(c, WorkoutMode.Edit, SampleData.Opened(SampleData.StrengthWorkout(Catalog(c))), error: WorkoutChecker(c).SaveFailed)),
        new("Screens/Edit Workout", "Discard prompt", c => Workout(c, WorkoutMode.Edit, SampleData.Opened(SampleData.StrengthWorkout(Catalog(c))), dialog: ("Discard changes?", "Discard"))),
        new("Screens/Edit Workout", "Delete prompt", c => Workout(c, WorkoutMode.Edit, SampleData.Opened(SampleData.StrengthWorkout(Catalog(c))), dialog: ("Delete this workout?", "Delete"))),
        new("Screens/Edit Workout", "Delete failed", c => Workout(c, WorkoutMode.Edit, SampleData.Opened(SampleData.StrengthWorkout(Catalog(c))), error: WorkoutChecker(c).DeleteFailed)),
        new("Screens/Edit Workout", "Loading", c => WorkoutLoad(c)),
        new("Screens/Edit Workout", "Load failed", c => WorkoutLoad(c, failed: true)),
        new("Screens/Edit Workout", "Not found", c => WorkoutLoad(c, notFound: true)),

        new("Screens/Exercise Picker", "No search text", c => Workout(c, WorkoutMode.New, SampleData.StrengthWorkout(Catalog(c)), pickerQuery: "")),
        new("Screens/Exercise Picker", "Results", c => Workout(c, WorkoutMode.New, SampleData.StrengthWorkout(Catalog(c)), pickerQuery: "dumbbell")),
        new("Screens/Exercise Picker", "No match", c => Workout(c, WorkoutMode.New, SampleData.StrengthWorkout(Catalog(c)), pickerQuery: "zumba")),

        new("Screens/History", "Empty", c => History(c, [], tappable: true)),
        new("Screens/History", "With workouts", c => History(c, SampleData.HistoryRows, tappable: true)),
        new("Screens/History", "Loading", c => History(c, [], tappable: false, loading: true)),
        new("Screens/History", "Load failed", c => History(c, [], tappable: false, failed: true)),

        new("Screens/Settings", "Empty", c => c.Render<SettingsView>(p => p.Add(x => x.OnBack, () => { })).FindAll("*")),

        new("Screens/Error", "Error", c => c.Render<ErrorView>(p => p.Add(x => x.OnRestart, () => { })).FindAll("*")),
    ];

    private static IExerciseCatalog Catalog(BunitContext context) => context.Services.GetRequiredService<IExerciseCatalog>();

    private static ISetChecker Checker(BunitContext context) => context.Services.GetRequiredService<ISetChecker>();

    private static IWorkoutChecker WorkoutChecker(BunitContext context) => context.Services.GetRequiredService<IWorkoutChecker>();

    private static IReadOnlyList<IElement> Dashboard(BunitContext context, DashboardSummary summary, bool loading = false, bool failed = false) =>
        context.Render<DashboardView>(p => p
            .Add(x => x.Summary, summary)
            .Add(x => x.Loading, loading)
            .Add(x => x.Failed, failed)
            .Add(x => x.OnRetry, () => { })
            .Add(x => x.OnNewWorkout, () => { })
            .Add(x => x.OnHistory, () => { })
            .Add(x => x.OnSettings, () => { }))
            .FindAll("*");

    private static IReadOnlyList<IElement> Workout(
        BunitContext context,
        WorkoutMode mode,
        List<ExerciseState> exercises,
        string? error = null,
        (string Title, string ConfirmText)? dialog = null,
        string? pickerQuery = null) =>
        context.Render<WorkoutView>(p =>
        {
            p.Add(x => x.Mode, mode)
                .Add(x => x.StartedAt, SampleData.Started)
                .Add(x => x.Time, new TimeSpan(0, 34, 12))
                .Add(x => x.Exercises, exercises)
                .Add(x => x.Error, error)
                .Add(x => x.ValidateSet, Checker(context).Check)
                .Add(x => x.OnAddExercise, () => { })
                .Add(x => x.OnRemoveExercise, (ExerciseState _) => { })
                .Add(x => x.OnSave, () => { })
                .Add(x => x.OnBack, () => { });
            if (mode == WorkoutMode.Edit)
            {
                p.Add(x => x.OnDelete, () => { });
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

            if (pickerQuery is not null)
            {
                p.Add<ExercisePicker>(x => x.Overlay, o => o
                    .Add(x => x.Query, pickerQuery)
                    .Add(x => x.Items, SampleData.PickerItems(Catalog(context), pickerQuery))
                    .Add(x => x.QueryChanged, (string _) => { })
                    .Add(x => x.OnPick, (PickerItem _) => { })
                    .Add(x => x.OnClose, () => { }));
            }
        }).FindAll("*");

    private static IReadOnlyList<IElement> WorkoutLoad(BunitContext context, bool failed = false, bool notFound = false) =>
        context.Render<WorkoutLoadView>(p => p
            .Add(x => x.Failed, failed)
            .Add(x => x.NotFound, notFound)
            .Add(x => x.OnRetry, () => { })
            .Add(x => x.OnBack, () => { }))
            .FindAll("*");

    private static IReadOnlyList<IElement> History(
        BunitContext context, IReadOnlyList<HistoryRow> rows, bool tappable, bool loading = false, bool failed = false) =>
        context.Render<HistoryView>(p =>
        {
            p.Add(x => x.Rows, rows)
                .Add(x => x.Loading, loading)
                .Add(x => x.Failed, failed)
                .Add(x => x.OnRetry, () => { })
                .Add(x => x.OnBack, () => { });
            if (tappable)
            {
                p.Add(x => x.OnOpen, (HistoryRow _) => { });
            }
        }).FindAll("*");
}
