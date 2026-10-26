using WorkoutLog.UI.Stories;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Services;
using WorkoutLog.UI;
using WorkoutLog.UI.Stories.Samples;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddUi();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IWorkoutService, SampleWorkoutService>();
builder.Services.AddSingleton<IDashboardService, SampleDashboardService>();
builder.Services.AddSingleton<IWeightHistoryService, SampleWeightHistoryService>();
builder.Services.AddSingleton<IExerciseCatalog, ExerciseCatalog>();

await builder.Build().RunAsync();
