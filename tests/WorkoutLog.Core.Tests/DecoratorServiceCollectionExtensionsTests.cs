using Microsoft.Extensions.DependencyInjection;

namespace WorkoutLog.Core.Tests;

public class DecoratorServiceCollectionExtensionsTests
{
    [Test]
    public void AddDecoratedSingleton_RegistersOnlyTheServiceAsASingleton()
    {
        var services = new ServiceCollection().AddDecoratedSingleton<IThing, ThingDecorator>(_ => new Thing());

        Assert.That(services.Select(s => (s.ServiceType, s.Lifetime)), Is.EqualTo(new[] { (typeof(IThing), ServiceLifetime.Singleton) }));
    }

    [Test]
    public void AddDecoratedSingleton_ResolvesTheDecoratorWrappingTheInner()
    {
        var inner = new Thing();
        var dependency = new Dependency();
        IServiceProvider? innerProvider = null;
        using var provider = new ServiceCollection()
            .AddSingleton(dependency)
            .AddDecoratedSingleton<IThing, ThingDecorator>(p =>
            {
                innerProvider = p;
                return inner;
            })
            .BuildServiceProvider();

        var thing = provider.GetRequiredService<IThing>();

        Assert.That(thing, Is.InstanceOf<ThingDecorator>());
        var decorator = (ThingDecorator)thing;
        Assert.Multiple(() =>
        {
            Assert.That(decorator.Inner, Is.SameAs(inner));
            Assert.That(decorator.Dependency, Is.SameAs(dependency));
            Assert.That(innerProvider, Is.Not.Null);
            Assert.That(innerProvider!.GetService<Dependency>(), Is.SameAs(dependency));
        });
    }

    [Test]
    public void AddDecoratedSingleton_CreatesOneInstance()
    {
        var created = 0;
        using var provider = new ServiceCollection()
            .AddSingleton<Dependency>()
            .AddDecoratedSingleton<IThing, ThingDecorator>(_ =>
            {
                created++;
                return new Thing();
            })
            .BuildServiceProvider();

        var first = provider.GetRequiredService<IThing>();

        Assert.Multiple(() =>
        {
            Assert.That(provider.GetRequiredService<IThing>(), Is.SameAs(first));
            Assert.That(created, Is.EqualTo(1));
        });
    }

    [Test]
    public void AddDecoratedSingleton_InnerCannotBeResolved()
    {
        using var provider = new ServiceCollection()
            .AddSingleton<Dependency>()
            .AddDecoratedSingleton<IThing, ThingDecorator>(_ => new Thing())
            .BuildServiceProvider();

        Assert.That(provider.GetService<Thing>(), Is.Null);
    }

    public interface IThing;

    public sealed class Dependency;

    public sealed class Thing : IThing;

    public sealed class ThingDecorator(IThing inner, Dependency dependency) : IThing
    {
        public IThing Inner { get; } = inner;

        public Dependency Dependency { get; } = dependency;
    }
}
