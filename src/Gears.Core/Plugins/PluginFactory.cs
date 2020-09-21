using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Gears
{

    public class PluginFactory : IPluginFactory
    {
        private readonly IServiceProvider serviceProvider;

        public PluginFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
        public TPlugin GetPlugin<TPlugin>(PluginConfiguration config) where TPlugin : IPlugin
        {
            var plugin = serviceProvider.GetServices<TPlugin>()
                .FirstOrDefault(s => s.Key.ToUpper() == config?.Type?.ToUpper());

            return plugin;

        }
    }
}
