using Application;
using CommandLine;
using Isun;
using Serilog;
using ConsoleApp;

static void CreateAndRunHost(CommandLineOptions commandLineOptions)
{
    var host = Host
        .CreateDefaultBuilder()
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
    .WithParsed(CreateAndRunHost);