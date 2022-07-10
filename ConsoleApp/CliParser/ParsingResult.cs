namespace ConsoleApp.CliParser;

public record ParsingResult(bool IsSuccess, HashSet<string> Arguments, string? Error = null);