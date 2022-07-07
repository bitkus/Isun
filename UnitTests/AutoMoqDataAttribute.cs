using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace UnitTests;

public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute()
        : base(() => new Fixture()
            .Customize(new CompositeCustomization(
                new AutoMoqCustomization(),
                new MemoryCacheCustomization())))
    {
    }
}

public class MemoryCacheCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        var options = fixture.Create<IOptions<MemoryCacheOptions>>();
        fixture.Register<IMemoryCache>(() => new MemoryCache(options));
    }
}