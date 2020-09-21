using System.Threading.Tasks;
using Gears.Formatters;

namespace Gears.Outputs
{
    public interface IOutput : IPlugin
    {
        Task<ReportResult> DeliveryAsync(dynamic data, OutputPluginConfiguration outputConfig, IFormatter formatter);
    }
}