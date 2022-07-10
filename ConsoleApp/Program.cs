using Application;
using ConsoleApp;
using Serilog;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using ConsoleApp.CliParser;

static void CreateAndRunHost(HashSet<string> arguments)
{
    var host = Host
        .CreateDefaultBuilder()
        .ConfigureAppConfiguration(configurationBuilder =>
        {
            var assembly = typeof(Program).GetTypeInfo().Assembly;
            configurationBuilder.AddJsonFile(new EmbeddedFileProvider(assembly), "appsettings.json", optional: false, false);
        })
        .UseSerilog((context, loggerConfiguration) =>
        {
            loggerConfiguration.WriteTo.File("log-.txt", rollingInterval: RollingInterval.Day);
            loggerConfiguration.MinimumLevel.Debug();
        })
        .ConfigureServices((context, services) =>
        {
            var configuration = context.Configuration;
            services
                .AddLogging(builder => builder.AddSerilog())
                .Configure<ConsoleAppOptions>(configuration.GetSection(nameof(ConsoleAppOptions)))
                .AddWeatherApplication(configuration, arguments)
                .AddTransient<IWriter, ConsoleWriter>()
                .AddHostedService<Worker>();
        })
        .Build();

    host.Run();
}

var parsingResult = Parser.Parse(Environment.CommandLine);
if (!parsingResult.IsSuccess)
{
    Console.WriteLine(parsingResult.Error);
    return;
}

CreateAndRunHost(parsingResult.Arguments);
