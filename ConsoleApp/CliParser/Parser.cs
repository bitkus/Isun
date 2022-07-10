namespace ConsoleApp.CliParser;

public static class Parser
{
    public static ParsingResult Parse(string commandLine)
    {
        if (string.IsNullOrEmpty(commandLine))
        {
            throw new ArgumentException("The environment command line was empty.");
        }

        var args = commandLine.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)[1..];
        if (args.Length == 0)
        {
            return new ParsingResult(false, args.ToHashSet(), "Please enter the --cities argument followed by the cities separated by comma or space you want the weather to be displayed for.");
        }

        if (args.Length > 0 && args[0] != "--cities")
        {
            return new ParsingResult(false, args.ToHashSet(), "The first and only argument for this program is --cities followed by the cities separated by comma or space for which to fetch the weather.");
        }

        if (args.Length == 1 && args[0] == "--cities")
        {
            return new ParsingResult(false, args.ToHashSet(), "Please enter some cities for which to fetch weather seaprated by comma or space.");
        }

        return new ParsingResult(true, args[1..].ToHashSet());
    }
}
