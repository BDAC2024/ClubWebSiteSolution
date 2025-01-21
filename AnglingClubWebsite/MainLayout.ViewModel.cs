using AnglingClubShared;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite
{
    public partial class MainLayoutViewModel : ViewModelBase, IRecipient<TurnOnDebugMessages>
    {
        public MainLayoutViewModel(
            IMessenger messenger) : base(messenger)
        {
            messenger.Register<TurnOnDebugMessages>(this);

            defineMenu();
        }

        [ObservableProperty]
        private string _testMessage = "Hello from MainLayoutViewModel";

        [ObservableProperty]
        private List<MenuItem> _menu = new List<MenuItem>();

        [ObservableProperty]
        private bool _showDebugMessages = true;

        #region Message Handlers
        public void Receive(TurnOnDebugMessages message)
        {
            ShowDebugMessages = message.YesOrNo;
        }

        #endregion Message Handlers

        #region Helper Methods

        public void defineMenu()
        {
            Menu.Add(new MenuItem { Id = "1", Name = "Welcome", NavigateUrl = "/" });
            Menu.Add(new MenuItem { Id = "2", Name = "News", NavigateUrl = "/News" });
            Menu.Add(new MenuItem { Id = "3", Name = "Club Waters" });
            Menu.Add(new MenuItem { Id = "4", Name = "Matches" });
            Menu.Add(new MenuItem { Id = "5", Name = "Standings", HasSubMenu = true });
            Menu.Add(new MenuItem { Id = "5.1", ParentId = "5", Name = "Leagues" });
            Menu.Add(new MenuItem { Id = "5.2", ParentId = "5", Name = "Weights" });
            Menu.Add(new MenuItem { Id = "5.3", ParentId = "5", Name = "Trophies" });
            Menu.Add(new MenuItem { Id = "6", Name = "Diary of Events" });
            Menu.Add(new MenuItem { Id = "7", Name = "Buy Online", HasSubMenu = true });
            Menu.Add(new MenuItem { Id = "7.1", ParentId = "7", Name = "Memberships" });
            Menu.Add(new MenuItem { Id = "7.2", ParentId = "7", Name = "Day Tickets" });
            Menu.Add(new MenuItem { Id = "7.3", ParentId = "7", Name = "Guest Tickets" });
            Menu.Add(new MenuItem { Id = "8", Name = "Club Info", HasSubMenu = true });
            Menu.Add(new MenuItem { Id = "8.1", ParentId = "8", Name = "Club Officers" });
            Menu.Add(new MenuItem { Id = "8.2", ParentId = "8", Name = "Rules", HasSubMenu = true });
            Menu.Add(new MenuItem { Id = "8.2.1", ParentId = "8.2", Name = "General" });
            Menu.Add(new MenuItem { Id = "8.2.2", ParentId = "8.2", Name = "Match" });
            Menu.Add(new MenuItem { Id = "8.2.3", ParentId = "8.2", Name = "Junior General" });
            Menu.Add(new MenuItem { Id = "8.2.4", ParentId = "8.2", Name = "Junior Match" });
            Menu.Add(new MenuItem { Id = "8.3", ParentId = "8", Name = "Club Forms" });
            Menu.Add(new MenuItem { Id = "8.4", ParentId = "8", Name = "Privacy Notice" });
            Menu.Add(new MenuItem { Id = "8.5", ParentId = "8", Name = "Environmental" });
            Menu.Add(new MenuItem { Id = "8.6", ParentId = "8", Name = "Angling Trust" });
            Menu.Add(new MenuItem { Id = "9", Name = "Admin", HasSubMenu = true });
            Menu.Add(new MenuItem { Id = "9.1", ParentId = "9", Name = "Members" });
            Menu.Add(new MenuItem { Id = "9.2", ParentId = "9", Name = "User Admins" });
            Menu.Add(new MenuItem { Id = "9.3", ParentId = "9", Name = "Payments" });
            Menu.Add(new MenuItem { Id = "10", Name = "My Details" });
            Menu.Add(new MenuItem { Id = "11", Name = "Logout" });

        }

        #endregion Helper Methods


    }

    #region Helper classes

    public class MenuItem
    {
        public string Id { get; set; } = "";
        public string? ParentId { get; set; } = null;
        public string Name { get; set; } = "";
        public bool Expanded { get; set; } = false;
        public bool HasSubMenu { get; set; } = false;
        public string? NavigateUrl { get; set; } = null;
    }

    #endregion Helper classes

}
