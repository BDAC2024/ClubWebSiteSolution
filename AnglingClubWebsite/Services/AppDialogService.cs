using AnglingClubShared;
using AnglingClubShared.Enums;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace Fishing.Client.Services
{
    public class AppDialogService : IAppDialogService
    {
        private readonly IMessenger _messenger;

        public AppDialogService(IMessenger messenger)
        {
            _messenger = messenger;
        }

        public void SendMessage(MessageState state, string title, string body, string closeButtonTitle = "Cancel")
        {
            _messenger.Send<ShowMessage>(new ShowMessage(state, title, body, closeButtonTitle));
        }
    }
}
