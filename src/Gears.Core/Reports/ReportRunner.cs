using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gears.Core.Plugins;
using Gears.Core.Services;
using Gears.Formatters;
using Gears.Outputs;
using Gears.Reports;
using Gears.Services;
using Gears.Transformers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gears
{

    public class ReportRunner : IReportRunner
    {
        private readonly IPluginFactory pluginFactory;
        private readonly IBatchServiceFactory batchServiceFactory;
        private readonly IConfiguration configuration;
        private readonly ReportConfigurationBuilder configurationBuilder;
        private readonly IPluginLoader pluginLoader;
        private readonly ILogger<ReportRunner> logger;

        public ReportRunner(
            IPluginFactory pluginFactory,
            IBatchServiceFactory batchServiceFactory,
            IConfiguration configuration,
            ReportConfigurationBuilder configurationBuilder,
            IPluginLoader pluginLoader,
            ILogger<ReportRunner> logger)
        {
            this.pluginFactory = pluginFactory;
            this.batchServiceFactory = batchServiceFactory;
            this.configuration = configuration;
            this.configurationBuilder = configurationBuilder;
            this.pluginLoader = pluginLoader;
            this.logger = logger;
        }

        public async Task<IDictionary<string, ReportResult>> RunReportsAsync(IEnumerable<string> reports)
        {
            var reportsToRun = configurationBuilder.BuildReportConfigurations(reports);

            if (reportsToRun?.Any() != true)
            {
                logger.LogInformation("No reports to run, exiting");
                return new Dictionary<string, ReportResult>()
                {
                    { "None",  new ReportResult { Status = 0 } }
                };
            }

            logger.LogInformation("Running Report(s): {ReportNames}", String.Join(", ", reportsToRun.Select(r => r.Key)));

            var pluginPaths = reportsToRun.SelectMany(r => r.Value.PluginPaths).Distinct().ToArray();
            pluginLoader.ScanAndLoad(pluginPaths);

            var results = new Dictionary<string, ReportResult>();
            foreach (var report in reportsToRun)
            {
                try
                {
                    // tasks.Add(
                    //     report.RunAsync(noBatch).ContinueWith(result =>
                    //     {
                    //         if (result.Exception != null)
                    //         {
                    //             logger.LogError(result.Exception, "Could not create {ReportName} report", report.Key);
                    //         }
                    //     })
                    //    );
                    var result = await RunReport(report);
                    results.Add(report.Key, result);
                }
                catch (System.Exception ex)
                {
                    results.Add(report.Key, new ReportResult { Status = 1, Message = ex.Message });
                    logger.LogError(ex, "Could not create {ReportName} report", report.Key);
                }
            }

            var succeeded = results.Where(r => r.Value.Status == 0);
            var failed = results.Where(r => r.Value.Status == 1);

            logger.LogInformation("Successful Reports: {Success}, Failed Reports: {Failed}", succeeded.Count(), failed.Count());

            if (succeeded.Count() > 0)
                logger.LogInformation("Successful Reports: {ReportName}", String.Join(",", succeeded.Select(i => i.Key)));

            if (failed.Count() > 0)
                logger.LogWarning("Failed Reports: {Failed}", String.Join(",", failed.Select(i => i.Key)));

            return results;
        }


        private async Task<ReportResult> RunReport(KeyValuePair<string, ReportConfiguration> reportConfig)
        {
            var reportResult = new ReportResult { Status = 0 };

            dynamic report = new { };

            var input = pluginFactory.GetPlugin<IInput>(reportConfig.Value.Input); //.GetValue<string>("Input:Type", "MSSQL"));
            if (input == null)
            {
                logger.LogCritical("No input could be found for this report: {ReportName}", reportConfig.Key);
                return new ReportResult
                {
                    Status = 1,
                    Message = "No input could be located for this report"
                };
            }

            var transformer = pluginFactory.GetPlugin<ITransformer>(reportConfig.Value.Transformer);

            logger.LogInformation("Processing of {ReportName} with {Input} {Transformer}",
                reportConfig.Key, input?.Key, transformer?.Key ?? "Default");

            var data = await input.GetDataAsync(reportConfig.Value.Input);

            if (data != null) // && data.Count > 0)
            {
                if (reportConfig.Value.Output.Count > 0)
                {

                    if (transformer != null)
                        data = transformer.Transform(reportConfig.Key, data, reportConfig.Value.Transformer);

                    foreach (var outputConfig in reportConfig.Value.Output)
                    {
                        try
                        {
                            var output = pluginFactory.GetPlugin<IOutput>(outputConfig.Value);

                            if (output != null)
                            {
                                var formatter = pluginFactory.GetPlugin<IFormatter>(outputConfig.Value.Formatter)
                                    ?? pluginFactory.GetPlugin<IFormatter>(new PluginConfiguration { Type = "Csv" });

                                if (formatter != null)
                                {

                                    var records = data;
                                    var batchService = batchServiceFactory.GetBatchService(reportConfig.Value.Input);

                                    if (reportConfig.Value.Input.Args.ShouldRunBatch())
                                    {
                                        records = batchService.FilterRecordsByOutput(outputConfig.Value, data);
                                    }

                                    if (HasData(records))
                                    {
                                        logger.LogInformation("Delivering results of {ReportName} to {Output} using {Formatter} format",
                                                  reportConfig.Key, output.Key, formatter.Key ?? "the default");


                                        ReportResult result = await output.DeliveryAsync(records, outputConfig.Value, formatter);

                                        logger.LogInformation("Delivered results of {ReportName} to {Output} using {Formatter} format",
                                                  reportConfig.Key, output.Key, formatter.Key ?? "the default");

                                        if (reportConfig.Value.Input.Args.ShouldRunBatch())
                                        {
                                            logger.LogInformation("Recording batch for {ReportName} - {OutputName}",
                                                reportConfig.Key, outputConfig.Key);

                                            await batchService.RecordBatchAsync(outputConfig.Value, result.Data);
                                        }
                                    }
                                    else
                                    {
                                        logger.LogInformation("No results after batch filter for {ReportName} with {Input} {Transformer}",
                                                          reportConfig.Key, input?.Key, transformer?.Key ?? "Default");
                                    }
                                }
                                else
                                {
                                    reportResult.Status = 1;

                                    logger.LogError("Could not locate formatter with key {FormatterName} for {ReportName}",
                                        outputConfig.Value?.Formatter?.Type, reportConfig.Key);
                                }
                            }
                            else
                            {
                                reportResult.Status = 1;

                                logger.LogError("Could not locate output with key {OutputName} for {ReportName}",
                                         outputConfig.Value?.Type, reportConfig.Key);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            logger.LogError(ex, "Could not write {ReportName} to {OutputName}", reportConfig.Key, outputConfig.Key);
                        }
                    }
                }
                else
                {
                    //No outputs
                    reportResult.Status = 1;
                    logger.LogWarning("No outputs enabled for {ReportName}", reportConfig.Key);
                }
            }
            else
            {
                //No results
                logger.LogInformation("No results for {ReportName} with {Input} {Transformer}",
                    reportConfig.Key, input?.Key, transformer?.Key ?? "Default");

            }
            return reportResult;
        }

        private static dynamic HasData(dynamic records)
        {
            var result = records != null;

            var enumerable = records as IEnumerable<object>;
            if (enumerable != null && enumerable.Count() == 0)
                result = false;

            return result;
        }
    }
}
