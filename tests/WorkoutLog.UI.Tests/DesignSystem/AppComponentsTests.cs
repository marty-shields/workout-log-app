namespace WorkoutLog.UI.Tests.DesignSystem;

public class AppComponentsTests
{
    [TestCase("public sealed class X : ComponentBase, IDisposable\n{", "X")]
    [TestCase("public class X<T> : ComponentBase\n{", "X")]
    [TestCase("public class X(ILogger log) : ComponentBase\n{", "X")]
    [TestCase("public class X : Microsoft.AspNetCore.Components.ComponentBase\n{", "X")]
    [TestCase("public class X : LayoutComponentBase\n{", "X")]
    [TestCase("public class X : OwningComponentBase<Foo>\n{", "X")]
    [TestCase("public class X : IComponent\n{", "X")]
    [TestCase("public class X<T> : ComponentBase where T : class\n{", "X")]
    [TestCase("public class A : ComponentBase { }\npublic class B : A { }", "A,B")]
    public void ClassesIn_FindsComponentClasses(string source, string expected)
    {
        Assert.That(AppComponents.ClassesIn(source), Is.EquivalentTo(expected.Split(',')));
    }

    [TestCase("public class X : IDisposable\n{")]
    [TestCase("public class X\n{")]
    [TestCase("public class X : Base\n{")]
    public void ClassesIn_IgnoresOtherClasses(string source)
    {
        Assert.That(AppComponents.ClassesIn(source), Is.Empty);
    }

    [Test]
    public void Found_CoversBothProjectsAndBothKindsOfComponent()
    {
        Assert.That(AppComponents.Names(), Is.SupersetOf(new[] { "ExerciseCard", "Routes", "SystemBarInsets" }));
    }

    [Test]
    public void Found_LeavesOutImportsFiles()
    {
        Assert.That(AppComponents.Names(), Has.None.EqualTo("_Imports"));
    }
}
