using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Gears.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gears.Outputs
{

    public class AzureBlobOutput : IOutput
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<AzureBlobOutput> logger;

        public AzureBlobOutput(IConfiguration configuration, ILogger<AzureBlobOutput> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        public string Key => "AzureBlob";


        public async Task<ReportResult> DeliveryAsync(dynamic data, OutputPluginConfiguration outputConfig, IFormatter formatter)
        {
            if (formatter is null)
            {
                throw new System.ArgumentNullException(nameof(formatter));
            }

            var blobConfig = outputConfig.Args;
            var connName = blobConfig.GetValue<string>("ConnectionString");

            string connectionString = configuration.GetConnectionString(connName);
            logger.LogInformation("Connecting to Azure Storage with {ConnectionName}",
                connName);

            var content = await formatter.GenerateContentAsync(outputConfig.Formatter, data);
            var containerName = blobConfig.GetValue<string>("Container");
            var blobName = blobConfig.GetValue<string>("Name");
            var contentType = blobConfig.GetValue<string>("ContentType");
            var overwrite = blobConfig.GetValue<bool>("Overwrite", true);
            var container = new BlobContainerClient(connectionString, containerName);

            if (!container.Exists())
                container.Create();

            if (string.IsNullOrEmpty(blobName))
                blobName = outputConfig.ReportName.GenerateFilename();

            var blob = container.GetBlobClient(blobName);

            await blob.UploadAsync(ValueExtensions.AsUTF8Stream(content.ToString()), overwrite);

            blob.SetHttpHeaders(new BlobHttpHeaders
            {
                ContentType = contentType
            });
            return new ReportResult
            {
                Status = 0,
                Message = "Uploaded to blob storage"
            };
        }
    }
}
