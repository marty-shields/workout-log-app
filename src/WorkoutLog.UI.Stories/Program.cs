using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Services;
using WorkoutLog.UI;
using WorkoutLog.UI.Haptics;
using WorkoutLog.UI.Import;
using WorkoutLog.UI.Stories;
using WorkoutLog.UI.Stories.Samples;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddUi();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IWorkoutService, SampleWorkoutService>();
builder.Services.AddSingleton<IExerciseCatalog, ExerciseCatalog>();
builder.Services.AddSingleton<IImportService, SampleImportService>();
builder.Services.AddSingleton<ILoggedExercisesService, SampleLoggedExercisesService>();
builder.Services.AddSingleton<ILastSessionService, SampleLastSessionService>();
builder.Services.AddSingleton<IImportFilePicker, SampleImportFilePicker>();
builder.Services.AddSingleton<IHaptics, SampleHaptics>();

await builder.Build().RunAsync();
