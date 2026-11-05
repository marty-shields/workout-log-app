using System.Text;
using WorkoutLog.UI.Stories.Stories.Foundations;

namespace WorkoutLog.UI.Tests.Stories;

public class AppIconSheetTests : ComponentTestBase
{
    private const string DataUri = "data:image/svg+xml;base64,";

    [Test]
    public void Render_ShowsTheAppsArt()
    {
        var cut = Context.Render<AppIconSheet>();

        var launcher = cut.FindAll(".mask img").Select(img => Decode(img.GetAttribute("src")!)).ToList();
        var themed = cut.FindAll(".silhouette").Select(e => e.GetAttribute("style")!).ToList();
        var monochromeUri = DataUri + Convert.ToBase64String(Encoding.UTF8.GetBytes(App("AppIcon", "appicon_monochrome.svg")));

        Assert.Multiple(() =>
        {
            Assert.That(launcher, Has.Count.EqualTo(8));
            Assert.That(launcher.Where((_, i) => i % 2 == 0), Is.All.EqualTo(App("AppIcon", "appicon.svg")));
            Assert.That(launcher.Where((_, i) => i % 2 == 1), Is.All.EqualTo(App("AppIcon", "appiconfg.svg")));
            Assert.That(themed, Has.Count.EqualTo(4).And.All.Contains(monochromeUri));
            Assert.That(Decode(cut.Find(".splash.light .splash-image").GetAttribute("src")!), Is.EqualTo(App("Splash", "splash.svg")));
            Assert.That(Decode(cut.Find(".splash.dark .splash-image").GetAttribute("src")!), Is.EqualTo(App("AppIcon", "appiconfg.svg")));
        });
    }

    private static string App(string folder, string file) => RepoFiles.Read("src", "WorkoutLog.App", "Resources", folder, file);

    private static string Decode(string src) => Encoding.UTF8.GetString(Convert.FromBase64String(src[DataUri.Length..]));
}
