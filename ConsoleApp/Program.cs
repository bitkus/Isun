using Application;
using CommandLine;
using Isun;
using Serilog;
using ConsoleApp;
using System.Reflection;
using Microsoft.Extensions.FileProviders;

static void CreateAndRunHost(CommandLineOptions commandLineOptions)
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
                .AddWeatherApplication(configuration, commandLineOptions.Cities!)
                .AddTransient<IWriter, ConsoleWriter>()
                .AddHostedService<Worker>();
        })
        .Build();

    host.Run();
}

Parser
    .Default
    .ParseArguments<CommandLineOptions>(args)
    .WithParsed(options =>
    {
        Normalize(options);
        CreateAndRunHost(options);
    });

void Normalize(CommandLineOptions options)
{
    options.Cities = options?.Cities?
        .Where(city => !string.IsNullOrEmpty(city))
        .Select(city => city
            .Trim(',', ' '));
}