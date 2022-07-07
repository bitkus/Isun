using Contracts;
using Persistence;

namespace UnitTests.Persistence;

public class WeatherForecastRepositoryTests
{
    [Theory, AutoMoqData]
    public void AddAndGet_Should_AddAndGet(
        WeatherForecastRepository weatherForecastRepository,
        WeatherForecast weatherForecast)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        weatherForecastRepository.Add(id, weatherForecast);
        var result = weatherForecastRepository.Get(id);

        // Assert
        Assert.Equal(weatherForecast, result);
    }
}