using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Org.BouncyCastle.Cms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;

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

        public void SendEmailToSupport(string subject, string textBody, List<string> attachmentFilenames = null, List<ImageAttachment> canvasAttachments = null)
        {
            SendEmail(new List<string> { _options.PrimaryEmailUsername }, subject, textBody, attachmentFilenames, canvasAttachments);
        }

        public void SendEmail(List<string> to, string subject, string textBody, List<string> attachmentFilenames = null, List<ImageAttachment> imageAttachments = null, List<StreamAttachment> streamAttachments = null)
        {
            if (to.Any())
            {
                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress(_options.PrimaryEmailFromName, _options.PrimaryEmailFromAddress));
                foreach (var recipient in to)
                {
                    mailMessage.To.Add(MailboxAddress.Parse(recipient));
                }

                if (!string.IsNullOrEmpty(_options.PrimaryEmailBCC))
                {
                    mailMessage.Bcc.Add(MailboxAddress.Parse(_options.PrimaryEmailBCC));
                }

                mailMessage.Subject = subject;

                var builder = new BodyBuilder();
                builder.TextBody = textBody;
                builder.HtmlBody = textBody + "<br><br>";

                if (imageAttachments != null)
                {
                    foreach (var att in imageAttachments)
                    {
                        var imageDataUrl = att.DataUrl; 
                        var imageBase64 = imageDataUrl.Replace("data:image/png;base64,", "");
                        byte[] temp_backToBytes = Convert.FromBase64String(imageBase64);
                        var imageAtt = new MimePart("image", "png", new MemoryStream(temp_backToBytes))
                        {
                            ContentTransferEncoding = ContentEncoding.Base64,
                            FileName = att.Filename
                        };

                        builder.Attachments.Add(imageAtt);

                        //builder.HtmlBody += $"<img src='{att.DataUrl}'/>";
                    }
                }

                if (attachmentFilenames != null)
                {
                    foreach (var att in attachmentFilenames)
                    {
                        builder.Attachments.Add(att);
                    }
                }

                if (streamAttachments != null)
                {
                    foreach (var att in streamAttachments)
                    {
                        builder.Attachments.Add(att.Filename, att.Bytes, new MimeKit.ContentType(att.ContentType, ""));
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
            catch (InvalidDataException)
            {
                throw;
            }
            catch (SmtpCommandException)
            {
                throw;
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning(ex, "Primary email sending failed - trying fallback email.");

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

                    repairEmailMessage.Body = builder.ToMessageBody();

                    sendViaSMTP(repairEmailMessage, _options.FallbackEmailHost, _options.FallbackEmailPort, _options.FallbackEmailUsername, _options.FallbackEmailPassword);

                }
                catch (System.Exception fex)
                {
                    _logger.LogError(fex, "Neither primary or fallback email sending worked.");
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

                try
                {
                    smtpClient.Send(mailMessage);
                }
                catch (SmtpCommandException smtpEx)
                {
                    if (smtpEx.ErrorCode == SmtpErrorCode.RecipientNotAccepted)
                    {
                        throw new InvalidDataException("Email address is invalid");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "sendViaSMTP failed.");
                    throw;
                }
                finally
                {
                    smtpClient.Disconnect(true);
                }
            }

        }


        #endregion Helper Methods

    }
}
