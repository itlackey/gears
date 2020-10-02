using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Data;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Diagnostics;
using Serilog;
using Gears.Services;
using Gears.Core.Plugins;
using System.Collections.Generic;

namespace Gears
{
    class Program
    {
        private static IConfiguration configuration;
        private static IHost host;

        static int Main(string[] args)
        {
            var configOption = new Option<string>("--config", description: "Provide a path to the config file to load");

            var result = 1;
            var command = new RootCommand
            {
                configOption,
                new Option<string[]>(aliases: new [] { "--reports", "-r"})
                {
                    Description = "Use 'all' or a space separated list of reports to run. Values must match a key from the Reports section of the configuration",
                    IsRequired = false
                },
                new Option<string[]>(aliases: new [] { "--clear-batches"})
                {
                    Description = "Use 'all' or a space separated list of reports to clear previous batch data from. Values must match a key from the Reports section of the configuration",
                    IsRequired = false
                },
            };
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("assets/config/logging.json")
                    .Build();

                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", "GEAR")
                    .ReadFrom.Configuration(config)
                    .CreateLogger();

                host = Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration(c =>
                   {
                       c.AddConfiguration(config);
                       c.AddJsonFile("assets/config/appsettings.prod.json", true);
                       c.AddJsonFile("assets/config/reports.json", true);
                       c.AddUserSecrets<Program>();


                       //var x = command.Parse(args).FindResultFor(configOption).Option.
                       string val = command.Parse(args).ValueForOption(configOption);
                       if (!string.IsNullOrEmpty(val))
                       {
                           c.AddJsonFile(val);
                       }

                   })
                    .ConfigureServices((hostContext, services) =>
                    {
                        configuration = hostContext.Configuration;
                        services.AddLogging(configure =>
                        {
                            configure.AddConfiguration(hostContext.Configuration);
                            configure.AddConsole();
                        });


                        services.AddTransient<IDbConnection>(provider =>
                        {
                            var connString = configuration.GetConnectionString("DefaultConnection");
                            Log.Logger.Debug("Connecting to db with {ConString}", connString);
                            var connection = new SqlConnection(connString);
                            try
                            {
                                connection.Open();
                                Log.Logger.Debug("Database connection is {State}", connection.State);
                                if (connection.State != ConnectionState.Open)
                                {
                                    Log.Logger.Error("Connection to database could not be opened, exiting.");
                                    return null;
                                }
                            }
                            catch (System.Exception ex)
                            {
                                Log.Logger.Error(ex, "There was an error while connecting to the database, exiting.");
                                return null;
                            }
                            return connection;
                        });

                        services
                        .RegisterServices()
                        .RegisterReports()
                        .RegisterInputs()
                        .RegisterTransformers()
                        .RegisterFormatters()
                        .RegisterOutputs();

                        services.AddSingleton<IServiceCollection>(services);
                        services.AddTransient<IPluginLoader, PluginLoader>();



                    })
                    .UseSerilog()
                    .Build();
                // var loader = host.Services.GetService<IPluginLoader>();// new PluginLoader(, services);
                // loader.ScanAndLoad(new string[] { "i2.gears.plugins/bin/Debug/netstandard2.0/i2.gears.plugins.dll" });


                command.Handler = CommandHandler.Create<string[], string[]>(RunAsync);
                result = command.InvokeAsync(args).Result;
            }
            catch (System.Exception ex)
            {
                Log.Logger.Fatal(ex, "Fatal error has occurred.");
                Log.Logger.Warning("Exiting...");
                result = 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
            return result;
        }


        async static Task<int> RunAsync(string[] reports, string[] clearBatches)
        {

            var timer = new Stopwatch();
            timer.Start();
            if (clearBatches?.Count() > 0)
            {
                ClearBatches(clearBatches);
                return 0;
            }
            else if (reports?.Count() > 0)
            {
                var pluginPaths = configuration.GetSection("Plugins:Paths").Get<string[]>();

                //.GetValue<IEnumerable<string>>("Paths");
                var pluginLoader = host.Services.GetRequiredService<IPluginLoader>();
                pluginLoader.ScanAndLoad(pluginPaths.ToArray());

                var reportRunner = host.Services.GetService<IReportRunner>();
                var results = await reportRunner.RunReportsAsync(reports);

                timer.Stop();
                Log.Logger.Information("Completed in {ElapsedTime}", timer.Elapsed);

                return 0;
            }
            else
            {
                Log.Logger.Warning("No reports were specified to run, please use --reports all to run all reports or specify a list of reports.");
                return 1;
            }

        }

        private static void ClearBatches(string[] clearBatches)
        {
            //ToDo fix this and support asOf param

            var batchService = host.Services.GetService<IBatchService>();
            // clearBatches.ToList().ForEach(r => batchService.ClearBatch(r));

        }
    }
}
