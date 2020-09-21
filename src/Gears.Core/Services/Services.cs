using Gears.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Configuration
{
    public static class Services
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            //services.AddTransient<BatchService>();
            services.AddTransient<TokenReplacementService>();
            //services.AddTransient<SmtpService>();
            return services;
        }
    }
}
