using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.IO;

namespace AnglingClubWebServices.Interfaces
{
    public interface IEmailService
    {
        void SendEmail(List<string> to, string subject, string textBody, List<string> attachmentFilenames = null, List<ImageAttachment> imageAttachments = null);
        void SendEmailToSupport(string subject, string textBody, List<string> attachmentFilenames = null, List<ImageAttachment> imageAttachments = null);
    }
}