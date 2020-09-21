using Microsoft.Extensions.Configuration;

namespace Gears.Transformers
{
    public interface ITransformer : IPlugin
    {
        dynamic Transform(string reportKey, dynamic input, PluginConfiguration transformerConfig);
    }
}
