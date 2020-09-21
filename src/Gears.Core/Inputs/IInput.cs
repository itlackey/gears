using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Gears
{
    public interface IInput : IPlugin
    {
        Task<dynamic> GetDataAsync(PluginConfiguration inputConfig);
        //Task RecordBatchAsync(OutputPluginConfiguration config, IEnumerable<dynamic> records);


    }

}
