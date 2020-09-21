using System;
using Gears.Transformers;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Configuration
{
    public static class Transformers
    {
        public static IServiceCollection RegisterTransformers(this IServiceCollection services)
        {

            services.AddTransient<ITransformer, DashboardTransformer>();
            services.AddTransient<ITransformer, JsonTransformer>();
            return services;
        }
    }
}
