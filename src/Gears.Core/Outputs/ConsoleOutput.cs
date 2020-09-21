using System;
using System.Threading.Tasks;
using Gears.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gears.Outputs
{
    public class ConsoleOutput : IOutput
    {
        private readonly ILogger<ConsoleOutput> logger;

        public ConsoleOutput(ILogger<ConsoleOutput> logger)
        {
            this.logger = logger;
        }

        public string Key => "Console";

        public async Task<ReportResult> DeliveryAsync(dynamic data, OutputPluginConfiguration outputConfig, IFormatter formatter)
        {
            if (formatter is null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            string content = await formatter.GenerateContentAsync(outputConfig.Formatter, data);

            logger.LogInformation("Results:\r\n{Content}", content);

            var result = new ReportResult { Status = 0 };
            return result;
        }
    }
}
