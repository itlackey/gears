using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gears.Reports
{

    public class ReportConfigurationBuilder : IReportConfigurationBuilder
    {
        private readonly IConfiguration configuration;
        private readonly IPluginFactory pluginFactory;
        private readonly ILogger<ReportConfigurationBuilder> logger;

        public ReportConfigurationBuilder(IConfiguration configuration,
                                          IPluginFactory pluginFactory,
                                          ILogger<ReportConfigurationBuilder> logger)
        {
            this.configuration = configuration;
            this.pluginFactory = pluginFactory;
            this.logger = logger;
        }
        public Dictionary<string, ReportConfiguration> BuildReportConfigurations(IEnumerable<string> reportKeys)
        {
            //ToDo: filter to enabled reports...
            //ToDo: default to running all enabled?
            if (reportKeys?.Any() != true)
                return new Dictionary<string, ReportConfiguration>();

            var reportsToRun = configuration.GetSection("Reports")
                .Get<Dictionary<string, ReportConfiguration>>(); // .GetChildren();

            if (!reportKeys.All(r => r == "all"))
                reportsToRun = reportsToRun
                    .Where(r => reportKeys.Any(x => x == r.Key))
                    .ToDictionary(x => x.Key, x => x.Value);

            reportsToRun = reportsToRun
                .Where(x => x.Value.Enabled)
                .ToDictionary(x => x.Key, x => x.Value);

            //ToDo: report disabled reports...

            reportsToRun = reportsToRun.ToDictionary(x => x.Key, x =>
            {
                if (x.Value.Output == null || x.Value.Output.Count == 0)
                {
                    //ToDo: include logic to look for simplified config settings like "Output": "Console"
                    var defaultOutputConfiguration = new OutputPluginConfiguration
                    {
                        Name = $"{x.Key}-Default",
                        ReportName = x.Key,
                        Args = x.Value.Args,
                        Type = x.Value.Args.GetValue<string>("OutputType", "Console"),
                        Formatter = new PluginConfiguration
                        {
                            Type = x.Value.Args.GetValue<string>("OutputFormat", "CSV")
                        }
                    };

                    if (!defaultOutputConfiguration.Args.GetValue<bool?>("AddTimestampToFilename").HasValue)
                        defaultOutputConfiguration.Args["AddTimestampToFilename"] = "true";

                    x.Value.Output = new Dictionary<string, OutputPluginConfiguration>()
                            {
                                {"Default",defaultOutputConfiguration}
                            };

                }

                if (x.Value.Input == null)
                {
                    x.Value.Input = new PluginConfiguration
                    {
                        Type = "MSSQL"
                    };
                }

                if (string.IsNullOrEmpty(x.Value.Input.Type))
                    x.Value.Input.Type = "MSSQL";

                if (x.Value.Input.Args == null)
                    x.Value.Input.Args = x.Value.Args;

                if (!x.Value.Input.Args.GetValue<bool?>("RunBatch").HasValue)
                    x.Value.Input.Args["RunBatch"] = "false";

                if (string.IsNullOrEmpty(x.Value.Input.Args.GetValue<string>("QueryPath")))
                    x.Value.Input.Args["QueryPath"] = $"Assets/queries/{x.Key}.sql";

                x.Value.Input.ReportName = x.Key;
                x.Value.ReportName = x.Key;

                x.Value.Output = x.Value.Output
                    .Where(o => o.Value.Enabled)
                    .ToDictionary(k => k.Key, v => v.Value);

                foreach (var item in x.Value.Output)
                {
                    if (item.Value.Formatter == null)
                    {
                        item.Value.Formatter = new PluginConfiguration
                        {
                            Type = "CSV"
                        };
                    }
                    //Use the key as the type if none is specified
                    if (string.IsNullOrEmpty(item.Value.Type))
                        item.Value.Type = item.Key;

                    item.Value.Name = item.Key;
                    item.Value.ReportName = x.Key;
                    item.Value.Formatter.ReportName = x.Key;
                }


                return x.Value;
            });
            return reportsToRun;
        }

    }
}
