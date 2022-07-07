namespace Infrastructure;

public record WeatherApiOptions
{
    public string? BaseUrl { get; init; }
    public string? User { get; set; }
    public string? Password { get; set; }
}