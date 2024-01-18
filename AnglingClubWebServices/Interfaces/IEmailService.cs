using AnglingClubWebServices.Models;
using System.Collections.Generic;

namespace AnglingClubWebServices.Interfaces
{
    public interface IEmailService
    {
        void SendEmail(List<string> to, string subject, string textBody, List<string> attachmentFilenames = null, List<CanvasAttachment> canvasAttachments = null);
        void SendEmailToSupport(string subject, string textBody, List<string> attachmentFilenames = null, List<CanvasAttachment> canvasAttachments = null);
    }
}