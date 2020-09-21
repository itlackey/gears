using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Gears.Services
{
    public class TokenReplacementService
    {
        private readonly ILogger<TokenReplacementService> logger;

        public TokenReplacementService(ILogger<TokenReplacementService> logger)
        {
            this.logger = logger;
        }

        public string ReplaceTokens(string template, dynamic record)
        {
            var builder = new StringBuilder(template);
            builder.Replace("{CurrentDate}", DateTime.Now.ToShortDateString());
            builder.Replace("{CurrentDateTime}", DateTime.Now.ToString());
            if (record != null)
            {
                try
                {
                    var dict = (IDictionary<string, object>)record;
                    foreach (var item in dict)
                    {
                        builder.Replace("{" + item.Key + "}", item.Value?.ToString() ?? "<Missing Value>");
                    }
                }
                catch (System.Exception)
                {
                    logger.LogInformation(
                     "Could not run replacements for {Template}, template data was not a valid dictionary", template);
                }

            }
            return builder.ToString();
        }
    }
}
