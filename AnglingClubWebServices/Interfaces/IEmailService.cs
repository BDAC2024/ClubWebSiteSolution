using System.Collections.Generic;

namespace AnglingClubWebServices.Interfaces
{
    public interface IEmailService
    {
        void SendEmail(string to, string subject, string textBody, List<string> attachmentFilenames = null);
        void SendEmailToSupport(string subject, string textBody, List<string> attachmentFilenames = null);
    }
}