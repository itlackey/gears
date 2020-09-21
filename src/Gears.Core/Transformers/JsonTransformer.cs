using System.Text.Json;

namespace Gears.Transformers
{
    class JsonTransformer : ITransformer
    {
        public string Key => "JSON";

        public dynamic Transform(string reportKey, dynamic input, PluginConfiguration transformerConfig)
            => JsonDocument.Parse(JsonSerializer.Serialize(input));

    }
}
