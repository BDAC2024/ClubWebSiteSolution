using AnglingClubShared.Enums;

namespace AnglingClubWebsite.Services
{
    public interface IAppDialogService
    {
        void SendMessage(MessageState state, string title, string body, string closeButtonTitle = "Cancel");
    }
}