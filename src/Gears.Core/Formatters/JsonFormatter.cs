using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Gears.Formatters
{
    public class JsonFormatter : IFormatter
    {
        public string Key => "JSON";

        public string ContentType => "application/json";

        public string DefaultFileExtension => ".json";

        public async Task<string> GenerateContentAsync(PluginConfiguration plugingConfig, dynamic records)
        {
            return await Task.Run(() => JsonSerializer.Serialize(records,
            new JsonSerializerOptions
            {
                WriteIndented = plugingConfig.Args.GetValue<bool>("prettyPrint", true)
            }));
        }
    }
}
