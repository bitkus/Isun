using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .Configure<WeatherApiOptions>(configuration.GetSection(nameof(WeatherApiOptions)))
            .AddSingleton<IWeatherApiClient, WeatherApiClient>(provider =>
            {
                var weatherApiOptions = provider.GetRequiredService<IOptions<WeatherApiOptions>>();
                var logger = provider.GetRequiredService<ILogger<WeatherApiClient>>();
                var weatherApiClient = new WeatherApiClient(weatherApiOptions, logger);
                weatherApiClient.Authorize().GetAwaiter().GetResult();
                return weatherApiClient;
            });
    }
}