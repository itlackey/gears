using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gears.Formatters;
using Gears.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gears.Outputs
{
    public class SmtpPerRecordOutput : IOutput
    {
        private readonly ILogger<SmtpPerRecordOutput> logger;
        private readonly SmtpService smtpService;
        private readonly TokenReplacementService tokenReplacementService;

        public SmtpPerRecordOutput(ILogger<SmtpPerRecordOutput> logger,
                                   SmtpService smtpService,
                                   TokenReplacementService tokenReplacementService)
        {
            this.logger = logger;
            this.smtpService = smtpService;
            this.tokenReplacementService = tokenReplacementService;
        }
        public string Key => "SmtpPerRecord";

        public async Task<ReportResult> DeliveryAsync(dynamic data, OutputPluginConfiguration outputConfig, IFormatter formatter)
        {
            if (data.Count > 0)
            {
                var isHtml = outputConfig.Args.GetValue<bool>($"IsHtml", true);
                var throttleEmail = outputConfig.Args.GetValue<bool>($"ThrottleEmail", true); ;
                var remainingRecords = data.Count;
                foreach (var record in data)
                {
                    var toAddress = GetToAddress(outputConfig.Args, record);
                    var subject = tokenReplacementService.ReplaceTokens(outputConfig.Args.GetValue<string>("Subject"), record);
                    var body = await formatter.GenerateContentAsync(outputConfig.Formatter, data);

                    smtpService.SendMail(toAddress, subject, body, isHtml, false, null);

                    remainingRecords--;
                    if (throttleEmail && remainingRecords > 5)
                    {
                        logger.LogInformation("Sleeping for 2 seconds to avoid smtp rate limits");
                        Thread.Sleep(2000);
                    }
                }
            }
            else
            {
                logger.LogInformation("No results for {ReportName}, skipping email", outputConfig.ReportName);
            }
            return new ReportResult { Status = 0 };
        }

        private string GetToAddress(IConfigurationSection args, dynamic record)
        {
            //ToDo: error handling!!
            var dict = (IDictionary<String, Object>)record;

            var toAddress = args.GetValue<string>($"ToAddress");
            var dynamicAddress = dict[toAddress]?.ToString();
            if (!String.IsNullOrEmpty(dynamicAddress))
                toAddress = dynamicAddress;

            return toAddress;
        }
    }
}
