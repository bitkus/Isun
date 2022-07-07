using Contracts;
using Flurl.Http;
using Flurl;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace Infrastructure;

public class WeatherApiClient : IWeatherApiClient
{
    private const string _weathersEndpoint = "weathers";
    private const string _citiesEndpoint = "cities";
    private const string _authorizeEndpoint = "authorize";

    private readonly IOptions<WeatherApiOptions> _weatherApiOptions;
    private readonly ILogger<WeatherApiClient> _logger;
    private string _token = string.Empty;

    public WeatherApiClient(IOptions<WeatherApiOptions> weatherApiOptions, ILogger<WeatherApiClient> logger)
    {
        _weatherApiOptions = weatherApiOptions;
        _logger = logger;
    }

    public async Task<IEnumerable<City>> GetAvailableCities()
    {
        _logger.LogInformation("GET {_citiesEndpoint}", _citiesEndpoint);
        var getCities = () => _weatherApiOptions.Value.BaseUrl
            .AppendPathSegments(_citiesEndpoint)
            .WithOAuthBearerToken(_token);

        var cities = await GetWithReauthorization<IEnumerable<string>>(getCities);
        return cities.Select(result => new City(result));
    }

    public async Task<WeatherForecast> GetWeatherForecast(City city)
    {
        _logger.LogInformation("GET {_weathersEndpoint} for {city}.", _weathersEndpoint, city);
        var getWeatherForecast = () => _weatherApiOptions.Value.BaseUrl
            .AppendPathSegments(_weathersEndpoint, city.Name)
            .WithOAuthBearerToken(_token);

        var weatherForecast = await GetWithReauthorization<WeatherForecast>(getWeatherForecast);
        return weatherForecast;
    }

    public async Task Authorize()
    {
        _logger.LogInformation("POST {_authorizeEndpoint}.", _authorizeEndpoint);
        var authorization = await _weatherApiOptions.Value.BaseUrl
            .AppendPathSegments(_authorizeEndpoint)
            .PostJsonAsync(new
            {
                Username = _weatherApiOptions.Value.User,
                Password = _weatherApiOptions.Value.Password
            })
            .ReceiveJson<WeatherApiAuthorization>();
        
        _token = authorization.Token;
    }

    private async Task<T> GetWithReauthorization<T>(Func<IFlurlRequest> getRequest)
    {
        var result = await getRequest().AllowHttpStatus(HttpStatusCode.Unauthorized).GetAsync();
        if (result.StatusCode == (int)HttpStatusCode.Unauthorized)
        {
            await Authorize();
            return await getRequest().GetJsonAsync<T>();
        }

        return await result.GetJsonAsync<T>();
    }
}