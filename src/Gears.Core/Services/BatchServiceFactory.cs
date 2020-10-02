using System;
using System.Linq;
using Gears.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Gears.Core.Services
{
    public interface IBatchServiceFactory
    {
        IBatchService GetBatchService(PluginConfiguration config);
    }

    public class BatchServiceFactory : IBatchServiceFactory
    {
        private readonly IServiceProvider serviceProvider;

        public BatchServiceFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
        public IBatchService GetBatchService(PluginConfiguration config)
        {
            var plugin = serviceProvider.GetServices<IBatchService>()
                .FirstOrDefault(s => s.Key.ToUpper() == config?.Type?.ToUpper());

            if (plugin == null)
                plugin = serviceProvider.GetServices<IBatchService>()
                    .FirstOrDefault(s => s.Key.ToUpper() == "DEFAULT");

            return plugin;

        }
    }
}
