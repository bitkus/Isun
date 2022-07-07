using Contracts;
using Microsoft.Extensions.Logging;

namespace Persistence;

public class WeatherForecastRepository : IRepository<WeatherForecast>
{
    private readonly Dictionary<Guid, PersistenceRecord<WeatherForecast>> _weatherForecastMap = new();
    private readonly ILogger<WeatherForecastRepository> _logger;

    public WeatherForecastRepository(ILogger<WeatherForecastRepository> logger)
    {
        _logger = logger;
    }

    public void Add(Guid id, WeatherForecast item)
    {
        _logger.LogInformation("Adding item id {id} to {repository}", id, nameof(WeatherForecastRepository));
        _weatherForecastMap[id] = new PersistenceRecord<WeatherForecast>(id, item);
    }

    public WeatherForecast? Get(Guid id)
    {
        _logger.LogInformation("Getting item id {id} from {repository}", id, nameof(WeatherForecastRepository));
        return _weatherForecastMap.ContainsKey(id) ? _weatherForecastMap[id].Data : null;
    }
}
