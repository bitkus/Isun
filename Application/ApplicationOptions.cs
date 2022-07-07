namespace Application;

public record ApplicationOptions
{
    public int? AvailableCityCacheTTLSeconds { get; init; }
}