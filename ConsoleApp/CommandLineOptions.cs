using CommandLine;

public record CommandLineOptions
{
    [Option("cities", Required = true, HelpText = "Specify cities for which weather information must be shown.")]
    public IEnumerable<string>? Cities { get; set; }
}
