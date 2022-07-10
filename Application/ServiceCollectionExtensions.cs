using Application.Services;
using Contracts;
using Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

namespace Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeatherApplication(this IServiceCollection services, IConfiguration configuration, HashSet<string> cities)
    {
        return services
            .AddInfrastructure(configuration)
            .AddPersistence()
            .Configure<ApplicationOptions>(configuration.GetSection(nameof(ApplicationOptions)))
            .AddSingleton(cities.Select(city => new City(city)))
            .AddSingleton<IMemoryCache, MemoryCache>()
            .AddTransient<IWeatherService, WeatherService>();
    }
}