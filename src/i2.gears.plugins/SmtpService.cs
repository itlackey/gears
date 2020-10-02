using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gears.Services
{
    public class SmtpService
    {
        private readonly ILogger<SmtpService> logger;
        private readonly IConfiguration configuration;

        public object[] JsonSerializermessage { get; private set; }

        public SmtpService(ILogger<SmtpService> logger,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }
        private SmtpConfiguration GetSmtpConfiguration()
        {
            var config = configuration.GetSection("SmtpConfiguration").Get<SmtpConfiguration>();
            return config;
        }
        public void SendMail(string toAddress, string subject, string body, bool isHtml = true, bool useBcc = false, IEnumerable<Attachment> attachments = null)
        {
            var smtpConfig = GetSmtpConfiguration();

            this.logger.LogDebug("Sending email to: {Email} with subject {Subject}", toAddress, subject);
            using (var mailClient = new SmtpClient(smtpConfig.Host, smtpConfig.Port))
            {
                mailClient.EnableSsl = smtpConfig.EnableSSL;

                if (!string.IsNullOrEmpty(smtpConfig.Username))
                    mailClient.Credentials = new NetworkCredential(smtpConfig.Username, smtpConfig.Password);

                var message = new MailMessage();
                message.From = new MailAddress(smtpConfig.FromAddress);
                if (useBcc)
                    message.Bcc.Add(toAddress);
                else
                    message.To.Add(toAddress);

                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;
                if (attachments != null)
                    foreach (var item in attachments)
                        message.Attachments.Add(item);

                mailClient.Send(message);
                this.logger.LogInformation("Mail sent to {Email} with subject {Subject}", toAddress, subject);

                // if (smtpConfig.ThrottleEmail)
                // {
                //     logger.LogInformation("Sleeping for 2 seconds to avoid smtp rate limits");
                //     Thread.Sleep(2000);
                // }
            }
        }
    }
}
