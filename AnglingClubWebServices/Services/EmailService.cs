using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Collections.Generic;
using System.Linq;

namespace AnglingClubWebServices.Services
{
    public class EmailService : IEmailService
    {
        #region Backing Fields

        private readonly EmailOptions _options;

        #endregion Backing Fields

        #region Constructors

        public EmailService(IOptions<EmailOptions> opts)
        {
            _options = opts.Value;
        }

        #endregion Constructors

        #region Methods

        public void SendEmailToSupport(string subject, string textBody, List<string> attachmentFilenames = null)
        {
            SendEmail(new List<string> { _options.EmailUsername }, subject, textBody, attachmentFilenames);
        }

        public void SendEmail(List<string> to, string subject, string textBody, List<string> attachmentFilenames = null)
        {
            if (to.Any())
            {
                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress(_options.EmailFromName, _options.EmailFromAddress));
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

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect(_options.EmailHost, _options.EmailPort, true);
                    smtpClient.Authenticate(_options.EmailUsername, _options.EmailPassword);
                    smtpClient.Send(mailMessage);
                    smtpClient.Disconnect(true);
                }
            }
        }

        #endregion Methods

        #region Helper Methods
        #endregion Helper Methods
    }
}
