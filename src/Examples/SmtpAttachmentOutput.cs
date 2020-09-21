using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Gears.Formatters;
using Gears.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gears.Outputs
{
    public class SmtpAttachmentOutput : IOutput
    {
        private readonly ILogger<SmtpAttachmentOutput> logger;
        private readonly SmtpService smtpService;
        private readonly TokenReplacementService tokenReplacementService;

        public SmtpAttachmentOutput(ILogger<SmtpAttachmentOutput> logger,
           SmtpService smtpService,
           TokenReplacementService tokenReplacementService)
        {
            this.logger = logger;
            this.smtpService = smtpService;
            this.tokenReplacementService = tokenReplacementService;
        }

        public string Key => "SmtpAttachment";

        public async Task<ReportResult> DeliveryAsync(dynamic data,
            OutputPluginConfiguration outputConfig, IFormatter formatter)
        {
            if (data.Count > 0)
            {
                var config = outputConfig.Args;

                string content = await formatter.GenerateContentAsync(outputConfig.Formatter, data);
                //var tempFilePath = Utils.WriteTempFile(reportConfig.Key, "");
                var toAddress = config.GetValue<string>($"ToAddress");
                var subject = config.GetValue<string>($"Subject"); //GetSubject(outputConfig.Args);
                var body = GetBody(outputConfig.Args, data);

                var isHtml = config.GetValue<bool>($"IsHtml", true);
                var useBcc = config.GetValue<bool>($"UseBcc", true);

                smtpService.SendMail(toAddress, subject, body, isHtml, useBcc,
                    new Attachment[]
                    {
                        new Attachment(content.AsUTF8Stream(),$"{outputConfig.ReportName.GenerateFilename("csv")}", "text/csv")
                    });
                logger.LogInformation("Emailed Report: {ReportName}|{ToAddress}|{Subject}",
                    outputConfig.ReportName, toAddress, subject);
            }
            else
            {
                logger.LogInformation("No results for {ReportName}, skipping email", outputConfig.ReportName);
            }
            return new ReportResult { Status = 0 };
        }

        private string GetBody(IConfigurationSection pluginConfiguration, IEnumerable<dynamic> records)
        {

            var template = pluginConfiguration.GetValue<string>($"Body");

            var loadBodyFromFile = pluginConfiguration.GetValue<bool>($"LoadBodyFromFile");
            if (loadBodyFromFile)
                template = File.ReadAllText(template);

            return tokenReplacementService.ReplaceTokens(template, null);
        }

        private string GetSubject(IConfigurationSection pluginConfiguration)
        {
            var subject = pluginConfiguration.GetValue<string>($"Subject");
            return tokenReplacementService.ReplaceTokens(subject, null);
        }

    }
}
