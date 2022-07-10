using ConsoleApp.CliParser;

namespace UnitTests.ConsoleApp.CliParser;

public class ParserTests
{
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Parse_Should_Throw_When_CommandLineIsEmpty(string commandLine)
    {
        // Arrange
        // Act
        // Assert
        var exception = Assert.Throws<ArgumentException>(() => Parser.Parse(commandLine));
        Assert.Equal("The environment command line was empty.", exception.Message);
    }

    [Theory]
    [InlineData("bla", "Please enter the --cities argument followed by the cities separated by comma or space you want the weather to be displayed for.")]
    [InlineData("bla bla", "The first and only argument for this program is --cities followed by the cities separated by comma or space for which to fetch the weather.")]
    [InlineData("bla bla --cities", "The first and only argument for this program is --cities followed by the cities separated by comma or space for which to fetch the weather.")]
    [InlineData("bla bla bla", "The first and only argument for this program is --cities followed by the cities separated by comma or space for which to fetch the weather.")]
    [InlineData("bla --cities", "Please enter some cities for which to fetch weather seaprated by comma or space.")]
    public void Parse_Should_ReturnError(string commandLine, string error)
    {
        // Arrange
        // Act
        var result = Parser.Parse(commandLine);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }

    [Theory]
    [InlineData("bla --cities a", "a")]
    [InlineData("bla --cities a,b", "a", "b")]
    [InlineData("bla --cities a, b", "a", "b")]
    [InlineData("bla --cities a,, b", "a", "b")]
    [InlineData("bla --cities a ,,b", "a", "b")]
    [InlineData("bla --cities a ,, b", "a", "b")]
    [InlineData("bla --cities a     b", "a", "b")]
    [InlineData("bla --cities ,a     b", "a", "b")]
    [InlineData("bla --cities a     b,", "a", "b")]
    [InlineData("bla --cities ,a     b,", "a", "b")]
    [InlineData("bla --cities ,,a     b,,", "a", "b")]
    [InlineData("bla --cities , ,a     b, ,", "a", "b")]
    [InlineData("bla --cities   , ,a     b, ,  ", "a", "b")]
    [InlineData("bla --cities   , ,a  c   b, ,  ", "a", "c", "b")]
    [InlineData("bla --cities   , ,a  c ,,,,  b, ,  ", "a", "c", "b")]
    [InlineData("bla --cities   , ,a    ,,, , c ,,,,  b, ,  ", "a", "c", "b")]
    [InlineData("bla --cities  ,,,, , ,a    ,,, , c ,,,,  b  ", "a", "c", "b")]
    [InlineData("bla --cities      a    ,,, , c ,,,,  b ,,, ", "a", "c", "b")]
    public void Parse_Should_ReturnSuccessAndDisregardCommasAndSpaces(string commandLine, params string[] arguments)
    {
        // Arrange
        // Act
        var result = Parser.Parse(commandLine);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(arguments.ToHashSet(), result.Arguments);
        Assert.Null(result.Error);
    }
}
