using Application;
using Application.Services;
using AutoFixture.Xunit2;
using Contracts;
using Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Persistence;

namespace UnitTests.Application.Services;

public class WeatherServiceTests
{
    [Theory, AutoMoqData]
    public async Task FetchWeather_Should_GetCitiesFromCache_When_CitiesAreInCache(
        [Frozen] Mock<IWeatherApiClient> weatherApiClientMock,
        [Frozen] IMemoryCache memoryCache,
        HashSet<City> cities,
        WeatherForecast weatherForecast,
        WeatherService weatherService)
    {
        // Arrange
        memoryCache.GetOrCreate("cities", _ => cities);
        weatherApiClientMock.Setup(m => m.GetWeatherForecast(It.IsAny<City>())).ReturnsAsync(weatherForecast);

        // Act
        await weatherService.FetchWeather(cities);

        // Assert
        weatherApiClientMock.Verify(m => m.GetAvailableCities(), Times.Never);
    }

    [Theory, AutoMoqData]
    public async Task FetchWeather_Should_RefreshCititesCache_When_ItHasExpired(
        [Frozen] Mock<IWeatherApiClient> weatherApiClientMock,
        [Frozen] Mock<IOptions<ApplicationOptions>> applicationOptionsProvider,
        ApplicationOptions applicationOptions,
        int availableCityCacheTTLSeconds,
        HashSet<City> cities,
        WeatherService weatherService)
    {
        // Arrange
        applicationOptions = applicationOptions with { AvailableCityCacheTTLSeconds = availableCityCacheTTLSeconds };
        applicationOptionsProvider.SetupGet(a => a.Value).Returns(applicationOptions);

        // Act
        await weatherService.FetchWeather(cities);

        // Assert
        weatherApiClientMock.Verify(m => m.GetAvailableCities());
    }

    [Theory, AutoMoqData]
    public async Task FetchWeather_Should_Throw_When_TTLOptionIsNull(
        HashSet<City> cities,
        [Frozen] Mock<IOptions<ApplicationOptions>> applicationOptionsProvider,
        ApplicationOptions applicationOptions,
        WeatherService weatherService)
    {
        // Arrange
        applicationOptions = applicationOptions with { AvailableCityCacheTTLSeconds = null };
        applicationOptionsProvider.SetupGet(a => a.Value).Returns(applicationOptions);

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() => weatherService.FetchWeather(cities));
    }

    [Theory, AutoMoqData]
    public async Task FetchWeather_Should_FetchAndSaveWeatherForCachedCities_When_CacheIsFull(
        [Frozen] Mock<IWeatherApiClient> weatherApiClientMock,
        [Frozen] IMemoryCache memoryCache,
        [Frozen] Mock<IRepository<WeatherForecast>> weatherForecastRepositorMock,
        [Frozen] Mock<IWriter> writerMock,
        Dictionary<City, WeatherForecast> cachedCityForecastMap,
        Dictionary<City, WeatherForecast> anyOtherCityForecastMap,
        WeatherService weatherService)
    {
        // Arrange
        var cachedCities = cachedCityForecastMap.Keys.ToHashSet();
        var otherCities = anyOtherCityForecastMap.Keys.ToHashSet();
        memoryCache.GetOrCreate("cities", _ => cachedCities);
        var maps = cachedCityForecastMap.Concat(anyOtherCityForecastMap).ToDictionary(x => x.Key, x => x.Value);
        weatherApiClientMock.Setup(m => m
            .GetWeatherForecast(It.IsAny<City>()))
            .ReturnsAsync<City, IWeatherApiClient, WeatherForecast>(city => maps[city]);

        // Act
        await weatherService.FetchWeather(otherCities);

        // Assert
        Assert.All(cachedCityForecastMap, cityForecast =>
        {
            weatherForecastRepositorMock.Verify(m => m.Add(It.IsAny<Guid>(), cityForecast.Value));
            writerMock.Verify(w => w.Write(cityForecast.Value.ToString()));
        });

        Assert.All(anyOtherCityForecastMap, cityForecast =>
        {
            weatherForecastRepositorMock.Verify(m => m.Add(It.IsAny<Guid>(), cityForecast.Value), Times.Never);
            writerMock.Verify(w => w.Write(cityForecast.Value.ToString()), Times.Never);
        });

        writerMock.Verify(w => w.Write("No weather is available for the passed cities."), Times.Never);
    }

    [Theory, AutoMoqData]
    public async Task FetchWeather_Should_DispayNoCities_When_NoCitiesAvailable(
        [Frozen] Mock<IWeatherApiClient> weatherApiClientMock,
        [Frozen] IMemoryCache memoryCache,
        [Frozen] Mock<IRepository<WeatherForecast>> weatherForecastRepositorMock,
        [Frozen] Mock<IWriter> writerMock,
        Dictionary<City, WeatherForecast> cachedCityForecastMap,
        Dictionary<City, WeatherForecast> anyOtherCityForecastMap,
        WeatherService weatherService)
    {
        // Arrange
        var cachedCities = cachedCityForecastMap.Keys.ToHashSet();
        var otherCities = anyOtherCityForecastMap.Keys.ToHashSet();
        memoryCache.GetOrCreate("cities", _ => Enumerable.Empty<City>().ToHashSet());
        var maps = cachedCityForecastMap.Concat(anyOtherCityForecastMap).ToDictionary(x => x.Key, x => x.Value);
        weatherApiClientMock.Setup(m => m
            .GetWeatherForecast(It.IsAny<City>()))
            .ReturnsAsync<City, IWeatherApiClient, WeatherForecast>(city => maps[city]);

        // Act
        await weatherService.FetchWeather(otherCities);

        // Assert
        Assert.All(cachedCityForecastMap, cityForecast =>
        {
            weatherForecastRepositorMock.Verify(m => m.Add(It.IsAny<Guid>(), cityForecast.Value), Times.Never);
            writerMock.Verify(w => w.Write(cityForecast.Value.ToString()), Times.Never);
        });

        Assert.All(anyOtherCityForecastMap, cityForecast =>
        {
            weatherForecastRepositorMock.Verify(m => m.Add(It.IsAny<Guid>(), cityForecast.Value), Times.Never);
            writerMock.Verify(w => w.Write(cityForecast.Value.ToString()), Times.Never);
        });

        writerMock.Verify(w => w.Write("No weather is available for the passed cities."), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task FetchWeather_Should_ReturnGivenCityAndApiCityIntersectionRegardlessOfCasing_When_CacheIsEmpty(
        [Frozen] Mock<IWeatherApiClient> weatherApiClientMock,
        [Frozen] IMemoryCache _,
        [Frozen] Mock<IRepository<WeatherForecast>> weatherForecastRepositorMock,
        [Frozen] Mock<IWriter> writerMock,
        [Frozen] Mock<IOptions<ApplicationOptions>> applicationOptionsProvider,
        ApplicationOptions applicationOptions,
        int availableCityCacheTTLSeconds,
        Dictionary<City, WeatherForecast> availableCityForecastMap,
        Dictionary<City, WeatherForecast> anyOtherCityForecastMap,
        WeatherService weatherService)
    {
        // Arrange
        var availableCities = availableCityForecastMap.Keys.ToHashSet();
        var otherCities = anyOtherCityForecastMap.Keys.ToHashSet();
        var maps = availableCityForecastMap.Concat(anyOtherCityForecastMap).ToDictionary(x => x.Key, x => x.Value);
        weatherApiClientMock.Setup(m => m
            .GetWeatherForecast(It.IsAny<City>()))
            .ReturnsAsync<City, IWeatherApiClient, WeatherForecast>(city => maps[city]);
        weatherApiClientMock.Setup(m => m
            .GetAvailableCities())
            .ReturnsAsync(availableCities);
        var requestedCities = new HashSet<City>();
        foreach (var city in availableCities.Concat(otherCities))
        {
            requestedCities.Add(city with { Name = city.Name.ToLowerInvariant() });
        }    

        applicationOptions = applicationOptions with { AvailableCityCacheTTLSeconds = availableCityCacheTTLSeconds };
        applicationOptionsProvider.SetupGet(a => a.Value).Returns(applicationOptions);

        // Act
        await weatherService.FetchWeather(requestedCities);

        // Assert
        Assert.All(availableCityForecastMap, cityForecast =>
        {
            weatherForecastRepositorMock.Verify(m => m.Add(It.IsAny<Guid>(), cityForecast.Value));
            writerMock.Verify(w => w.Write(cityForecast.Value.ToString()));
        });

        Assert.All(anyOtherCityForecastMap, cityForecast =>
        {
            weatherForecastRepositorMock.Verify(m => m.Add(It.IsAny<Guid>(), cityForecast.Value), Times.Never);
            writerMock.Verify(w => w.Write(cityForecast.Value.ToString()), Times.Never);
        });

        writerMock.Verify(w => w.Write("No weather is available for the passed cities."), Times.Never);
    }
}
