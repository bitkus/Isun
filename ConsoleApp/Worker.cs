using Application.Services;
using Contracts;
using Microsoft.Extensions.Options;

namespace ConsoleApp;

public class Worker : BackgroundService
{
    private readonly ConsoleAppOptions _consoleAppOptions;
    private readonly ILogger<Worker> _logger;
    private readonly IEnumerable<City> _cities;
    private readonly IWeatherService _weatherService;

    public Worker(ILogger<Worker> logger, IEnumerable<City> cities, IWeatherService weatherService, IOptions<ConsoleAppOptions> consoleAppOptionProvider)
    {
        _logger = logger;
        _cities = cities;
        _weatherService = weatherService;
        _consoleAppOptions = consoleAppOptionProvider.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting console weather app...");
        while (!stoppingToken.IsCancellationRequested)
        {
            _ = _weatherService.FetchWeather(_cities);
            await Task.Delay(
                TimeSpan.FromSeconds(
                    _consoleAppOptions.DelayBetweenWeatherFetchSeconds ?? 
                        throw new ArgumentException("Delay between weather forecast fetching is not set.")),
                stoppingToken);
        }
    }
}