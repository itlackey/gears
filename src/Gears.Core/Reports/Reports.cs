using Gears;
using Gears.Reports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Configuration
{
    public static class ServiceRegistrationExtensions
    {


        public static IServiceCollection RegisterReports(this IServiceCollection services)
        {
            services.AddTransient<IPluginFactory, PluginFactory>();
            services.AddTransient<IReportRunner, ReportRunner>();
            services.AddTransient<ReportConfigurationBuilder>();

            return services;
        }

    }

}
