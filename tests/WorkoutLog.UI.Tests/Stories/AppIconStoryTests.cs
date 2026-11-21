using System.Text;
using System.Text.RegularExpressions;
using Shields.UI.Stories.Shared;
using WorkoutLog.UI.Stories.Stories.Foundations;

namespace WorkoutLog.UI.Tests.Stories;

public partial class AppIconStoryTests : ComponentTestBase
{
    private const string DataUri = "data:image/svg+xml;base64,";

    [Test]
    public void Story_GivesEachSlotTheAppsArt()
    {
        var cut = Context.Render<AppIconSheet>(p => p
            .Add(s => s.Background, AppIcon_stories.Background)
            .Add(s => s.Foreground, AppIcon_stories.Foreground)
            .Add(s => s.Monochrome, AppIcon_stories.Monochrome)
            .Add(s => s.SplashLight, AppIcon_stories.SplashLight)
            .Add(s => s.SplashDark, AppIcon_stories.SplashDark));

        var launcher = cut.FindAll(".mask img").Select(img => img.GetAttribute("src")!).ToList();
        var themed = cut.FindAll(".silhouette").Select(e => e.GetAttribute("style")!).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(launcher, Has.Count.EqualTo(8));
            Assert.That(launcher.Where((_, i) => i % 2 == 0), Is.All.EqualTo(Uri("AppIcon", "appicon.svg")));
            Assert.That(launcher.Where((_, i) => i % 2 == 1), Is.All.EqualTo(Uri("AppIcon", "appiconfg.svg")));
            Assert.That(themed, Has.Count.EqualTo(4).And.All.Contains(Uri("AppIcon", "appicon_monochrome.svg")));
            Assert.That(cut.Find(".splash.light .splash-image").GetAttribute("src"), Is.EqualTo(Uri("Splash", "splash.svg")));
            Assert.That(cut.Find(".splash.dark .splash-image").GetAttribute("src"), Is.EqualTo(Uri("AppIcon", "appiconfg.svg")));
        });
    }

    [Test]
    public void StoryTemplate_PassesEachFieldToTheSheetParameterOfTheSameName()
    {
        var story = RepoFiles.Read("src", "WorkoutLog.UI.Stories", "Stories", "Foundations", "AppIcon.stories.razor");

        var sheet = Sheet().Match(story).Groups[1].Value;
        var passed = Parameter().Matches(sheet).ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);

        Assert.That(passed, Is.EqualTo(new Dictionary<string, string>
        {
            ["Background"] = "Background",
            ["Foreground"] = "Foreground",
            ["Monochrome"] = "Monochrome",
            ["SplashLight"] = "SplashLight",
            ["SplashDark"] = "SplashDark",
        }));
    }

    private static string Uri(string folder, string file) =>
        DataUri + Convert.ToBase64String(Encoding.UTF8.GetBytes(RepoFiles.Read("src", "WorkoutLog.App", "Resources", folder, file)));

    [GeneratedRegex(@"<AppIconSheet\s([^>]*)/>")]
    private static partial Regex Sheet();

    [GeneratedRegex(@"(\w+)=""@(\w+)""")]
    private static partial Regex Parameter();
}
