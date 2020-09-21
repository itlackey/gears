using Gears.Outputs;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Configuration
{
    public static class Outputs
    {
        public static IServiceCollection RegisterOutputs(this IServiceCollection services)
        {

            services.AddTransient<IOutput, ConsoleOutput>();
            services.AddTransient<IOutput, FileOutput>();
            // services.AddTransient<IOutput, SmtpAttachmentOutput>();
            // services.AddTransient<IOutput, SmtpPerRecordOutput>();
            // services.AddTransient<IOutput, AzureBlobOutput>();
            // services.AddTransient<IOutput, SftpOutput>();
            return services;
        }
    }
}
