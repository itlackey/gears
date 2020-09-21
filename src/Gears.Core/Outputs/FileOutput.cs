using System;
using System.IO;
using System.Threading.Tasks;
using Gears.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gears.Outputs
{
    public class FileOutput : IOutput
    {
        private readonly ILogger<FileOutput> logger;

        public FileOutput(ILogger<FileOutput> logger)
        {
            this.logger = logger;
        }
        public string Key => "File";

        public async Task<ReportResult> DeliveryAsync(dynamic data, OutputPluginConfiguration outputConfig, IFormatter formatter)
        {
            if (formatter is null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            string content = (await formatter.GenerateContentAsync(outputConfig.Formatter, data)) ?? data?.ToString();


            var outputFilename = outputConfig.Args?.GetValue<string>("Filename")
               ?? $"{outputConfig.ReportName}{formatter.DefaultFileExtension}";

            if (outputConfig.Args?.GetValue<bool>("AddTimestampToFilename", true) ?? true)
                outputFilename = outputFilename.GenerateFilename();

            var outputPath = outputConfig.Args?.GetValue<string>("Path", ".output") ?? ".output";
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            var path = Path.Combine(outputPath, outputFilename);
            File.WriteAllText(path, content);

            logger.LogInformation("Wrote {ReportName} report to {Path}", outputConfig.ReportName, path);

            return new ReportResult { Status = 0, Message = path };
        }
    }
}
