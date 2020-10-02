using System;
using System.Collections;
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

        public Task<string> GenerateContentAsync(PluginConfiguration plugingConfig, dynamic records)
        {
            object data = records;
            var content = JsonSerializer.Serialize(data,
            new JsonSerializerOptions
            {
                WriteIndented = plugingConfig?.Args?.GetValue<bool>("prettyPrint", true) ?? true
            });

            return Task.FromResult(content);
        }
    }
}
