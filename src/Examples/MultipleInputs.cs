using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Gears
{
    public class MultipleInputs : IInput
    {
        private readonly IPluginFactory pluginFactory;

        public MultipleInputs(IPluginFactory pluginFactory)
        {
            this.pluginFactory = pluginFactory;
        }
        public string Key => "Multiple";


        public async Task<dynamic> GetDataAsync(PluginConfiguration config)
        {
            dynamic data = new { };
            foreach (var source in config.Args.GetSection("Sources").GetChildren())
            {
                var inputConfig = source.Get<PluginConfiguration>();
                var input = pluginFactory.GetPlugin<IInput>(inputConfig);
                data[source.Key] = await input.GetDataAsync(inputConfig);
            }
            return data;
        }

    }

}
