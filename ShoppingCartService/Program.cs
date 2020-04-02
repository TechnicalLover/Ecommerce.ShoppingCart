using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using ShoppingCartService.Models.Constants;

namespace ShoppingCartService
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                var webHost = CreateHostBuilder(args).Build();
                webHost.Run();
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(CreateConfigurationBuilder)
                // use Serilog instead of AspNetCore's build-in logging
                // .ConfigureLogging(logging =>
                // {
                //     logging.ClearProviders();
                //     logging.AddConsole();
                // })
                .UseSerilog(ConfigureLogger)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseKestrel(builder => builder.AllowSynchronousIO = true)
                        .UseStartup<Startup>();
                });

        public static void CreateConfigurationBuilder(IConfigurationBuilder configBuilder) =>
            configBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables();

        public static void ConfigureLogger(HostBuilderContext hostingContext, LoggerConfiguration loggerConfiguration) =>
            loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Debug()
                //.WriteTo.Console(outputTemplate: LoggingTemplate.Default)
                .WriteTo.ColoredConsole(
                    LogEventLevel.Verbose,
                    LoggingTemplate.DefaultWithCorrelationToken
                );
    }
}
