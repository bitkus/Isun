using Contracts;
using Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence;

namespace Application.Services;

public class WeatherService : IWeatherService
{
    private readonly IWeatherApiClient _weatherApiClient;
    private readonly IRepository<WeatherForecast> _weatherForecastRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly IOptions<ApplicationOptions> _applicationOptions;
    private readonly ILogger<WeatherService> _logger;
    private readonly IWriter _writer;
    private const string _cacheKey = "cities";

    public WeatherService(
        IWeatherApiClient weatherApiClient,
        IRepository<WeatherForecast> weatherForecastRepository,
        IMemoryCache memoryCache,
        ILogger<WeatherService> logger,
        IOptions<ApplicationOptions> applicationOptions,
        IWriter writer)
    {
        _weatherApiClient = weatherApiClient;
        _weatherForecastRepository = weatherForecastRepository;
        _memoryCache = memoryCache;
        _logger = logger;
        _applicationOptions = applicationOptions;
        _writer = writer;
    }

    public async Task FetchWeather(IEnumerable<City> cities)
    {
        _logger.LogInformation("Fetching new weather.");
        var availableCities = await GetAvailableCitiesFromCacheOrApi(cities);
        foreach (var city in availableCities)
        {
            var weatherForecast = await _weatherApiClient.GetWeatherForecast(city);
            var weatherForecastId = Guid.NewGuid();
            _weatherForecastRepository.Add(weatherForecastId, weatherForecast);
            _writer.Write(weatherForecast.ToString());
        }

        if (availableCities.Count == 0)
        {
            _writer.Write("No weather is available for the passed cities.");
        }
    }

    private async Task<HashSet<City>> GetAvailableCitiesFromCacheOrApi(IEnumerable<City> cities)
    {
        var availableCities = await _memoryCache.GetOrCreateAsync(_cacheKey, async entry =>
        {
            _logger.LogInformation("Refreshing available city cache.");
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(
                _applicationOptions.Value.AvailableCityCacheTTLSeconds ?? 
                    throw new ArgumentException("The city refresh cache TTL is not set"));
            var availableCities = await _weatherApiClient.GetAvailableCities();
            return availableCities.Intersect(cities).ToHashSet();
        });

        return availableCities;
    }
}
