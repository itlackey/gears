using Gears.Formatters;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Configuration
{
    public static class Formatters
    {
        public static IServiceCollection RegisterFormatters(this IServiceCollection services)
        {
            //https://gist.github.com/bbarry/ae9ac27e56306005ff2285a6d4c4344e
            services.AddTransient<IFormatter, CsvFormatter>();
            services.AddTransient<IFormatter, JsonFormatter>();
            services.AddTransient<IFormatter, TextHtmlFormatter>();
            return services;

        }

    }
}
