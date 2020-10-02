using System;
using Gears;
using Gears.Inputs;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Configuration
{
    public static class Inputs
    {
        public static IServiceCollection RegisterInputs(this IServiceCollection services)
        {
            //services.AddTransient<IInput, MsSqlInput>();
            services.AddTransient<IInput, JsonFileInput>();
            return services;
        }
    }
}
