using System.Data;
using System.IO;
using System.Threading.Tasks;
using Gears.Services;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gears
{
    public static class MsSqlInputConfigurationExtensions
    {


        public static string[] GetHashedColumns(this IConfigurationSection inputConfigItem)
        {
            return inputConfigItem.GetSection("HashedColumns").Get<string[]>();
        }
        public static string GetQuery(this IConfigurationSection pluginConfigSection, string defaultFilename = "")
        {
            if (string.IsNullOrEmpty(defaultFilename))
                defaultFilename = $"{pluginConfigSection.Key}.sql";

            var path = pluginConfigSection.GetValue<string>("QueryPath")
                ?? $"assets/queries/{defaultFilename}";

            if (File.Exists(path))
                return File.ReadAllText(path);
            else
                return string.Empty;

        }

    }
    internal class MsSqlInput : IInput
    {
        private readonly BatchService batchService;
        private readonly IConfiguration configuration;
        private readonly ILogger<MsSqlInput> logger;

        public MsSqlInput(BatchService batchService,
            IConfiguration configuration,
            ILogger<MsSqlInput> logger)
        {
            this.batchService = batchService;
            this.configuration = configuration;
            this.logger = logger;
        }
        public string Key => "MSSQL";

        public async Task<dynamic> GetDataAsync(PluginConfiguration pluginConfiguration)
        {

            //var pluginConfiguration = config.GetSection("Input:Args:MSSQL");
            var query = pluginConfiguration.Args?.GetQuery()
                ?? $"{pluginConfiguration.ReportName}.sql";

            if (pluginConfiguration.Args?.ShouldRunBatch() ?? true)
            {
                query = batchService.GenerateInputBatchQuery(pluginConfiguration);
            }

            var connStringName = pluginConfiguration.Args?.GetValue<string>("ConnectionString")
                            ?? "DefaultConnection";

            var connString = configuration.GetConnectionString(connStringName);
            logger.LogInformation("Connecting to db with {ConnectionName}", connStringName);
            using (var connection = new SqlConnection(connString))
            {
                try
                {
                    connection.Open();
                    logger.LogDebug("Database connection is {State}", connection.State);
                    if (connection.State != ConnectionState.Open)
                    {
                        logger.LogError("Connection to database could not be opened, exiting.");
                        return null;
                    }
                    var records = await connection.QueryAsync(query);
                    return records;
                }
                catch (System.Exception ex)
                {
                    logger.LogError(ex, "There was an error while connecting to the database, exiting.\r\n{Query}", query);
                    return null;
                }


            }


            //logger.LogDebug("Query to run: \r\n{Query}", query);


        }

        // public Task RecordBatchAsync(PluginConfiguration config, System.Collections.Generic.IEnumerable<dynamic> records)
        // {
        //     return batchService.RecordBatchAsync(config, records);
        // }
    }

}
