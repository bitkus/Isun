using AutoFixture.Xunit2;
using Contracts;
using Flurl.Http;
using Flurl.Http.Testing;
using Infrastructure;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;

namespace UnitTests.Infrastructure;

public class WeatherApiClientTests
{
    private const string _baseUrl = "http://bla.com";
    private const string _user = "bla";
    private const string _password = "bla";

    [Theory, AutoMoqData]
    public async Task GetAvailableCities_Should_CallCitiesEndpoint(
        [Frozen] Mock<IOptions<WeatherApiOptions>> weatherApiOptionsMock,
        IEnumerable<City> response,
        WeatherApiClient weatherApiClient)
    {
        // Arrange
        using var httpTest = new HttpTest();
        MockApiOptions(weatherApiOptionsMock);
        httpTest.RespondWithJson(response.Select(c => c.Name), (int)HttpStatusCode.OK);

        // Act
        var result = await weatherApiClient.GetAvailableCities();

        // Assert
        httpTest.ShouldHaveCalled($"{_baseUrl}/cities");
        foreach (var item in result)
        {
            Assert.Contains(item, response);
        }
    }

    [Theory, AutoMoqData]
    public async Task GetAvailableCities_Should_Reauthorize_When_Unauthorized(
        [Frozen] Mock<IOptions<WeatherApiOptions>> weatherApiOptionsMock,
        IEnumerable<City> citiesResponse,
        WeatherApiAuthorization authorizationResponse,
        WeatherApiClient weatherApiClient)
    {
        // Arrange
        using var httpTest = new HttpTest();
        MockApiOptions(weatherApiOptionsMock);
        httpTest
            .RespondWithJson(citiesResponse.Select(c => c.Name), (int)HttpStatusCode.Unauthorized)
            .RespondWithJson(authorizationResponse, (int)HttpStatusCode.OK)
            .RespondWithJson(citiesResponse.Select(c => c.Name), (int)HttpStatusCode.OK);

        // Act
        var result = await weatherApiClient.GetAvailableCities();

        // Assert
        httpTest.ShouldHaveCalled($"{_baseUrl}/authorize");
        httpTest.ShouldHaveCalled($"{_baseUrl}/cities");
        foreach (var item in result)
        {
            Assert.Contains(item, citiesResponse);
        }
    }

    [Theory, AutoMoqData]
    public async Task GetAvailableCities_Should_Throw_When_UnauthorizedAndNotReauthorized(
        [Frozen] Mock<IOptions<WeatherApiOptions>> weatherApiOptionsMock,
        IEnumerable<City> citiesResponse,
        WeatherApiAuthorization authorizationResponse,
        WeatherApiClient weatherApiClient)
    {
        // Arrange
        using var httpTest = new HttpTest();
        MockApiOptions(weatherApiOptionsMock);
        httpTest
            .RespondWithJson(citiesResponse.Select(c => c.Name), (int)HttpStatusCode.Unauthorized)
            .RespondWithJson(authorizationResponse, (int)HttpStatusCode.Unauthorized);

        // Act
        // Assert
        await Assert.ThrowsAsync<FlurlHttpException>(() => weatherApiClient.GetAvailableCities());
        httpTest.ShouldHaveCalled($"{_baseUrl}/authorize");
        httpTest.ShouldHaveCalled($"{_baseUrl}/cities");
    }

    [Theory, AutoMoqData]
    public async Task GetWeatherForecast_Should_CallWeatherEndpoint(
        [Frozen] Mock<IOptions<WeatherApiOptions>> weatherApiOptionsMock,
        WeatherForecast response,
        City city,
        WeatherApiClient weatherApiClient)
    {
        // Arrange
        using var httpTest = new HttpTest();
        MockApiOptions(weatherApiOptionsMock);
        httpTest.RespondWithJson(response, (int)HttpStatusCode.OK);

        // Act
        var result = await weatherApiClient.GetWeatherForecast(city);

        // Assert
        httpTest.ShouldHaveCalled($"{_baseUrl}/weathers/{city.Name}");
        Assert.Equal(response, result);
    }

    [Theory, AutoMoqData]
    public async Task GetWeatherForecast_Should_Reauthorize_When_Unauthorizedd(
        [Frozen] Mock<IOptions<WeatherApiOptions>> weatherApiOptionsMock,
        WeatherForecast weatherForecastResponse,
        City city,
        WeatherApiAuthorization authorizationResponse,
        WeatherApiClient weatherApiClient)
    {
        // Arrange
        using var httpTest = new HttpTest();
        MockApiOptions(weatherApiOptionsMock);
        httpTest
            .RespondWithJson(weatherForecastResponse, (int)HttpStatusCode.Unauthorized)
            .RespondWithJson(authorizationResponse, (int)HttpStatusCode.OK)
            .RespondWithJson(weatherForecastResponse, (int)HttpStatusCode.OK);

        // Act
        var result = await weatherApiClient.GetWeatherForecast(city);

        // Assert
        httpTest.ShouldHaveCalled($"{_baseUrl}/authorize");
        httpTest.ShouldHaveCalled($"{_baseUrl}/weathers/{city.Name}");
        Assert.Equal(weatherForecastResponse, result);
    }

    [Theory, AutoMoqData]
    public async Task GetWeatherForecast_Should_Throw_When_UnauthorizedAndNotReauthorized(
        [Frozen] Mock<IOptions<WeatherApiOptions>> weatherApiOptionsMock,
        WeatherForecast weatherForecastResponse,
        City city,
        WeatherApiAuthorization authorizationResponse,
        WeatherApiClient weatherApiClient)
    {
        // Arrange
        using var httpTest = new HttpTest();
        MockApiOptions(weatherApiOptionsMock);
        httpTest
            .RespondWithJson(weatherForecastResponse, (int)HttpStatusCode.Unauthorized)
            .RespondWithJson(authorizationResponse, (int)HttpStatusCode.Unauthorized);

        // Act
        // Assert
        await Assert.ThrowsAsync<FlurlHttpException>(() => weatherApiClient.GetWeatherForecast(city));
        httpTest.ShouldHaveCalled($"{_baseUrl}/authorize");
        httpTest.ShouldHaveCalled($"{_baseUrl}/weathers/{city.Name}");
    }

    [Theory, AutoMoqData]
    public async Task Authorize_Should_CallAuthorizeEndpoint(
        [Frozen] Mock<IOptions<WeatherApiOptions>> weatherApiOptionsMock,
        WeatherApiAuthorization response,
        WeatherApiClient weatherApiClient)
    {
        // Arrange
        using var httpTest = new HttpTest();
        MockApiOptions(weatherApiOptionsMock);
        httpTest.RespondWithJson(response, (int)HttpStatusCode.OK);

        // Act
        await weatherApiClient.Authorize();

        // Assert
        httpTest.ShouldHaveCalled($"{_baseUrl}/authorize");
    }

    private void MockApiOptions(Mock<IOptions<WeatherApiOptions>> weatherApiOptionsMock)
    {
        weatherApiOptionsMock.SetupGet(m => m.Value).Returns(new WeatherApiOptions
        {
            BaseUrl = _baseUrl,
            User = _user,
            Password = _password
        });
    }
}
