using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnglingClubWebServices.Services
{
    public class EmailService : IEmailService
    {
        #region Backing Fields

        private readonly EmailOptions _options;
        private readonly ILogger<EmailService> _logger;

        #endregion Backing Fields

        #region Constructors

        public EmailService(IOptions<EmailOptions> opts,
            ILoggerFactory loggerFactory)
        {
            _options = opts.Value;
            _logger = loggerFactory.CreateLogger<EmailService>();
        }

        #endregion Constructors

        #region Methods

        public void SendEmailToSupport(string subject, string textBody, List<string> attachmentFilenames = null)
        {
            SendEmail(new List<string> { _options.PrimaryEmailUsername }, subject, textBody, attachmentFilenames);
        }

        public void SendEmail(List<string> to, string subject, string textBody, List<string> attachmentFilenames = null)
        {
            if (to.Any())
            {
                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress(_options.PrimaryEmailFromName, _options.PrimaryEmailFromAddress));
                foreach (var recipient in to)
                {
                    mailMessage.To.Add(MailboxAddress.Parse(recipient));
                }
                mailMessage.Subject = subject;

                var builder = new BodyBuilder();
                builder.TextBody = textBody;
                builder.HtmlBody = textBody;

                if (attachmentFilenames != null)
                {
                    foreach (var att in attachmentFilenames)
                    {
                        builder.Attachments.Add(att);
                    }
                }

                mailMessage.Body = builder.ToMessageBody();

                sendWithFallback(mailMessage);
            }
        }

        #endregion Methods

        #region Helper Methods

        private void sendWithFallback(MimeMessage mailMessage)
        {
            try
            {
                sendViaSMTP(mailMessage, _options.PrimaryEmailHost, _options.PrimaryEmailPort, _options.PrimaryEmailUsername, _options.PrimaryEmailPassword);
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning("Primary email sending failed - trying fallback email.", ex);

                try
                {
                    // Email failed via primary email account. Re-send a fallback account
                    mailMessage.From.Remove(new MailboxAddress(_options.PrimaryEmailFromName, _options.PrimaryEmailFromAddress));
                    mailMessage.From.Add(new MailboxAddress(_options.FallbackEmailFromName, _options.FallbackEmailFromAddress));

                    sendViaSMTP(mailMessage, _options.FallbackEmailHost, _options.FallbackEmailPort, _options.FallbackEmailUsername, _options.FallbackEmailPassword);

                    // Now inform developer to repair the primary email account
                    var repairEmailMessage = new MimeMessage();
                    repairEmailMessage.From.Add(new MailboxAddress(_options.FallbackEmailFromName, _options.FallbackEmailFromAddress));
                    repairEmailMessage.To.Add(MailboxAddress.Parse(_options.FallbackEmailUsername));
                    repairEmailMessage.Subject = "Primary email account failed";

                    var builder = new BodyBuilder();
                    builder.TextBody = $"The primary email account could not be used. However, the email has been sent using a fallback account. The error was {ex.Message}" +
                                        Environment.NewLine +
                                        Environment.NewLine +
                                        $"Google may have disabled less-secure app access (e.g. login via username/password). This can be checked and re-enabled here: {_options.PrimaryEmailRepairUrl}";

                    builder.HtmlBody = builder.TextBody;

                    repairEmailMessage.Body = builder.ToMessageBody();

                    sendViaSMTP(repairEmailMessage, _options.FallbackEmailHost, _options.FallbackEmailPort, _options.FallbackEmailUsername, _options.FallbackEmailPassword);

                }
                catch (System.Exception fex)
                {
                    _logger.LogError("Neither primary or fallback email sending worked.", fex);
                    throw;
                }
            }

        }

        private void sendViaSMTP(MimeMessage mailMessage, string emailHost, int emailPort, string emailUsername, string emailPassword)
        {
            using (var smtpClient = new SmtpClient())
            {
                if (emailHost.ToLower().Contains("outlook"))
                {
                    smtpClient.Connect(emailHost, emailPort, MailKit.Security.SecureSocketOptions.StartTls);
                }
                else
                {
                    smtpClient.Connect(emailHost, emailPort, true);
                }

                smtpClient.Authenticate(emailUsername, emailPassword);
                smtpClient.Send(mailMessage);
                smtpClient.Disconnect(true);
            }

        }


        #endregion Helper Methods
    }
}
