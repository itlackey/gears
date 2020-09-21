using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Gears.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Gears.Formatters
{
    public class TextHtmlFormatter : IFormatter
    {
        private readonly ILogger<TextHtmlFormatter> logger;
        private readonly TokenReplacementService tokenReplacementService;

        public TextHtmlFormatter(ILogger<TextHtmlFormatter> logger, TokenReplacementService tokenReplacementService)
        {
            this.logger = logger;
            this.tokenReplacementService = tokenReplacementService;
        }
        public string Key => "TextHtml";

        public string ContentType => "text/html";

        public string DefaultFileExtension => ".html";

        public Task<string> GenerateContentAsync(PluginConfiguration pluginConfig, dynamic records)
        {
            var htmlBuilder = new StringBuilder();

            var template = pluginConfig.Args.GetValue<string>($"Path");

            htmlBuilder.Append(File.ReadAllText(template));

            var data = (records as List<object>)?.FirstOrDefault();

            var html = tokenReplacementService.ReplaceTokens(htmlBuilder.ToString(), data ?? new { }).ToString();

            return Task.FromResult(html);
        }
    }
}
