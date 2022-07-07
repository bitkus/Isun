using Application;

namespace ConsoleApp;

public class ConsoleWriter : IWriter
{
    public void Write(string text) => Console.WriteLine(text);
}
