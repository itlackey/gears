using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Gears.Services
{
    public class DefaultBatchServie : IBatchService
    {
        public string Key => "Default";

        public void ClearBatch(string reportKey, string outputKey, DateTime? reportedOnOrAfter = null)
        {
            //No Op
        }

        public IEnumerable<dynamic> FilterRecordsByOutput(OutputPluginConfiguration output, IEnumerable<dynamic> records)
        {
            return records;
        }

        public string GenerateInputBatchQuery(PluginConfiguration config)
        {
            throw new NotImplementedException();
        }

        public Task RecordBatchAsync(OutputPluginConfiguration config, IEnumerable<dynamic> records)
        {
            //NoOp
            return Task.FromResult(0);
        }
    }
}