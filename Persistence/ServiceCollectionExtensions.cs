using Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        return services
            .AddSingleton<IRepository<WeatherForecast>, WeatherForecastRepository>();
    }
}
