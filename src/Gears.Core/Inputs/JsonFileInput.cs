using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gears.Inputs
{
    public class JsonFileInput : IInput
    {
        private readonly ILogger<JsonFileInput> logger;

        public JsonFileInput(ILogger<JsonFileInput> logger)
        {
            this.logger = logger;
        }
        public string Key => "JsonFile";
        public async Task<dynamic> GetDataAsync(PluginConfiguration inputConfig)
        {

            var path = inputConfig.Args?.GetValue<string>("Path", ".input") ?? ".input";
            if (!File.Exists(path))
                return null;

            var content = File.ReadAllText(path);

            logger.LogInformation("Reading data for {ReportName} from {Path}", inputConfig.ReportName, path);

            var doc = await JsonSerializer.DeserializeAsync<IEnumerable<ExpandoObject>>(content.AsUTF8Stream());
            return doc;
        }
    }
}
