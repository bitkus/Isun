using Contracts;

namespace Infrastructure;

public interface IWeatherApiClient
{
    Task<WeatherForecast> GetWeatherForecast(City city);
    Task<IEnumerable<City>> GetAvailableCities();
    Task Authorize();
}