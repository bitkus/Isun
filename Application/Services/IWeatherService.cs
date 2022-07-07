using Contracts;

namespace Application.Services;

public interface IWeatherService
{
    Task FetchWeather(IEnumerable<City> cities);
}