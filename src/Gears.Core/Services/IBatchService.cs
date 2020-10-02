using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Gears.Services
{
    public interface IBatchService
    {
        string Key { get; }
        void ClearBatch(string reportKey, string outputKey, DateTime? reportedOnOrAfter = null);
        IEnumerable<dynamic> FilterRecordsByOutput(OutputPluginConfiguration output, IEnumerable<dynamic> records);
        string GenerateInputBatchQuery(PluginConfiguration config);
        Task RecordBatchAsync(OutputPluginConfiguration config, IEnumerable<dynamic> records);
    }
}