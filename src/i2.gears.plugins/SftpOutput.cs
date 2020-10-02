using System.IO;
using System.Threading.Tasks;
using Gears.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace Gears.Outputs
{
    public class SftpOutput : IOutput
    {
        private readonly ILogger<SftpOutput> logger;

        public SftpOutput(ILogger<SftpOutput> logger)
        {
            this.logger = logger;
        }

        public string Key => "SFTP";

        public async Task<ReportResult> DeliveryAsync(dynamic data, OutputPluginConfiguration outputConfig, IFormatter formatter)
        {
            if (formatter is null)
            {
                throw new System.ArgumentNullException(nameof(formatter));
            }

            dynamic result = new ReportResult { Status = 1 };
            try
            {
                if (data != null)
                {

                    string content = await formatter.GenerateContentAsync(outputConfig.Formatter, data);
                    var tempFileName = outputConfig.ReportName.GenerateFilename(outputConfig.Formatter?.Type);
                    var ftpHost = outputConfig.Args.GetValue<string>($"Hostname");
                    var ftpPort = outputConfig.Args.GetValue<int>($"Port");
                    var ftpUsername = outputConfig.Args.GetValue<string>($"Username");
                    var ftpPassword = outputConfig.Args.GetValue<string>($"Password");
                    var ftpPath = outputConfig.Args.GetValue<string>($"Path", "");

                    using (var client = new SftpClient(ftpHost, ftpPort, ftpUsername, ftpPassword))
                    {
                        client.Connect();
                        using (var fileStream = content.AsUTF8Stream())
                        {
                            try
                            {

                                logger.LogInformation("Uploading {Key} report to sftp://{FtpHost}/{FtpPath}/{FtpFileName}",
                                   outputConfig.ReportName, ftpHost, ftpPath, tempFileName);

                                if (!string.IsNullOrEmpty(ftpPath))
                                    client.ChangeDirectory(ftpPath);

                                client.UploadFile(fileStream, tempFileName);
                                fileStream.Close();
                                logger.LogInformation("Uploaded {Key} report to sftp://{FtpHost}/{FtpPath}/{FtpFileName}",
                                                outputConfig.ReportName, ftpHost, ftpPath, tempFileName);
                            }
                            catch (System.Exception ftpEx)
                            {
                                logger.LogError(ftpEx, "Could not upload file");
                            }

                        }
                        logger.LogDebug("Disconnecting");
                        client.Disconnect();
                        logger.LogDebug("Disconnected");
                        logger.LogDebug("Disposing SFTP client");
                    }
                    logger.LogDebug("Disposed SFTP client");
                }
                else
                {
                    logger.LogInformation("No results for {ReportName}, skipping upload", outputConfig.ReportName);
                }
                result.Status = 0;
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Could not upload {Key} report via ftp", outputConfig.ReportName);
                result.Status = 1;
                result.Message = ex.Message;
            }

            return result;
        }


    }
}
