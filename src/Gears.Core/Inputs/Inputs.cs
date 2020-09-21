using System;
using Gears;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Configuration
{
    public static class Inputs
    {
        public static IServiceCollection RegisterInputs(this IServiceCollection services)
        {
            //services.AddTransient<IInput, MsSqlInput>();
            return services;
        }
    }
}
