using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Mailjet.Client;
using Mailjet.Client.Resources;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Encodings;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Cms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

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
            if (_options.UseEmailAPI)
            {
                _logger.LogWarning($" log 136.0.1 - email - sending via api");
                sendViaApi(to, subject, textBody, attachmentFilenames, imageAttachments, streamAttachments).Wait();
            }
            else
            {
                _logger.LogWarning($" log 136.0.2 - email - NOT sending via api");

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

                    _logger.LogWarning($" log 136.0.3 - email");

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

                    // No longer used
                    //sendWithFallback(mailMessage);

                    _logger.LogWarning($" log 136.0.4 - email");

                    // Mailjet SMTP timing out on AWS Lambda so using the fallback (outlook) settings. Although these will be removed soon.
                    mailMessage.From.Remove(new MailboxAddress(_options.PrimaryEmailFromName, _options.PrimaryEmailFromAddress));
                    mailMessage.From.Add(new MailboxAddress(_options.FallbackEmailFromName, _options.FallbackEmailFromAddress));

                    try
                    {
                        sendViaSMTP(mailMessage, _options.FallbackEmailHost, _options.FallbackEmailPort, _options.FallbackEmailUsername, _options.FallbackEmailPassword);

                        _logger.LogWarning($" log 136.0.5 - email worked");

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $" log 136.0.6 - email failed");
                    }


                }
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

        private async Task sendViaApi(List<string> to, string subject, string textBody, List<string> attachmentFilenames = null, List<ImageAttachment> imageAttachments = null, List<StreamAttachment> streamAttachments = null)
        {
            var toArray = new JArray();
            foreach (var recip in to)
            {
                toArray.Add(JObject.Parse
                (
                    @"{""Email"":""" + recip + "\", " +
                    @"""Name"":""" + recip +
                @"""}"));
            }

            var bccArray = new JArray();
            if (!string.IsNullOrEmpty(_options.PrimaryEmailBCC))
            {
                bccArray.Add(JObject.Parse
                (
                    @"{""Email"":""" + _options.PrimaryEmailBCC + "\", " +
                    @"""Name"":""" + _options.PrimaryEmailBCC +
                @"""}"));
            }

            var attStreamArray = new JArray();
            if (streamAttachments != null)
            {
                foreach (var att in streamAttachments)
                {
                    attStreamArray.Add(JObject.Parse
                    (
                        @"{""ContentType"":""" + att.ContentType + "\", " +
                        @"""Filename"":""" + att.Filename + "\", " +
                        @"""Base64Content"":""" + Convert.ToBase64String(att.Bytes) +
                    @"""}"));
                }
            }

            if (imageAttachments != null)
            {
                foreach (var att in imageAttachments)
                {
                    var imageDataUrl = att.DataUrl;
                    var imageBase64 = imageDataUrl.Replace("data:image/png;base64,", "");

                    attStreamArray.Add(JObject.Parse
                    (
                        @"{""ContentType"":""" + "data:image/png" + "\", " +
                        @"""Filename"":""" + att.Filename + "\", " +
                        @"""Base64Content"":""" + imageBase64 +
                    @"""}"));

                }
            }

            MailjetClient client = new MailjetClient(_options.EmailAPIPublicKey, _options.EmailAPIPrivateKey);
            MailjetRequest request = new MailjetRequest
            {
                Resource = SendV31.Resource
            }
            .Property(Send.Messages, new JArray 
            {
                new JObject 
                {
                    { "From", new JObject 
                        {
                            {"Email", _options.PrimaryEmailFromAddress},
                            {"Name", _options.PrimaryEmailFromName}
                        }
                    },
                    {"To", toArray},
                    {"Bcc", bccArray},
                    {"Subject", subject},
                    //{"TextPart", ""},
                    {"HTMLPart", textBody},
                    {"Attachments", attStreamArray }
                }
            })
            .Property(Send.SandboxMode, false); 

            MailjetResponse response = await client.PostAsync(request);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Email sent via API to: {to.First()}");
                //_logger.LogWarning(response.GetData());
            }
            else
            {
                _logger.LogError($"Email failed via API to: {to.First()}");
                _logger.LogError(string.Format("Email StatusCode: {0}\n", response.StatusCode));
                _logger.LogError(string.Format("Email ErrorInfo: {0}\n", response.GetErrorInfo()));
                _logger.LogError(string.Format("Email ErrorMessage: {0}\n", response.GetErrorMessage()));
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
