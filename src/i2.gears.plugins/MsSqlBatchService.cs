using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Data;
using Dapper;
using System.Linq;
using System;
using Microsoft.Extensions.Configuration;

namespace Gears.Services
{

    public class BatchService : IBatchService
    {

        //ToDo add LastUpdated and OutputName
        private readonly ILogger<BatchService> logger;
        private readonly IDbConnection connection;
        public BatchService(ILogger<BatchService> logger, IDbConnection connection)
        {
            this.logger = logger;
            this.connection = connection;
        }

        public string Key => "MsSql";

        public void ClearBatch(string reportKey, string outputKey, DateTime? reportedOnOrAfter = null)
        {
            if (reportKey == "all")
                connection.Execute("TRUNCATE TABLE BatchRecords");

            else
                connection.Execute("DELETE FROM BatchRecords Where Report = @reportKey AND Output = @outputKey",
                    new { reportKey, outputKey });
        }

        public IEnumerable<dynamic> FilterRecordsByOutput(OutputPluginConfiguration output, IEnumerable<dynamic> records)
        {
            var batchRecords = connection.Query<BatchRecord>
                ($"SELECT * FROM BatchRecords WHERE Report = '{output.ReportName} AND Output = '{output.Name}'");

            foreach (var item in records)
                if (batchRecords.Count(b => b.Hash == item.Hash) == 0)
                    yield return item;
        }

        public string GenerateInputBatchQuery(PluginConfiguration config)
        {
            //     query = $"SELECT Q.* FROM (\r\n{item.Query}\r\n) as Q "
            //    + "LEFT JOIN BatchRecords as B ON B.Id = Q.Id AND B.Hash = Q.Hash "
            //    + $"AND B.Report = '{item.Key}' "
            //    + "WHERE B.Id IS NULL";

            var args = config.Args;
            var hashColumn = $"CONVERT(varbinary(8000), HASHBYTES('MD5', (SELECT Q.* FOR XML RAW)))";
            if (args.GetHashedColumns()?.Count() > 0)
            {
                hashColumn = $"CONVERT(varbinary(8000), HASHBYTES('MD5', CONCAT({String.Join(",", args.GetHashedColumns()) })))";
            }

            return $"SELECT Q.*, {hashColumn} as Hash "
            + $"FROM (\r\n{args.GetQuery()}\r\n) as Q "
             + "LEFT JOIN BatchRecords as B ON B.Id = Q.Id "
             + $"AND B.Hash = {hashColumn} "
             + $"AND B.Report = '{config.ReportName}' "
             + "WHERE B.Id IS NULL";
        }

        public async Task RecordBatchAsync(OutputPluginConfiguration config, IEnumerable<dynamic> records)
        {
            try
            {

                var previousBatchRecords = await connection.QueryAsync<BatchRecord>
                    ("select * from BatchRecords Where Report = @reportKey", new { reportKey = config.ReportName });

                foreach (var record in records)
                {
                    byte[] hash = record.Hash;
                    if (previousBatchRecords.Any(pr => pr.Id == record.Id))
                        await connection.ExecuteAsync("UPDATE BatchRecords SET Hash = @hash, LastUpdated = getdate() "
                        + "WHERE Id = @id AND Report = @reportName AND Output = @outputName",
                            new { hash, record.Id, config.ReportName, outputName = config.Name });
                    else
                        await connection.ExecuteAsync("INSERT INTO BatchRecords (Hash, Id, Report) "
                        + " VALUES (@hash,@id,@reportName,@outputName)",
                           new { hash, record.Id, config.ReportName, outputName = config.Name });
                }

            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Could not record batch for {ReportName} - {OutputName}", config.ReportName, config.Name);
            }
        }

    }
}