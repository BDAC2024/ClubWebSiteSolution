using AnglingClubShared;
using AnglingClubWebsite.SharedComponents;
using AnglingClubShared.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Syncfusion.Blazor.Notifications;
using AnglingClubShared.Entities;
using AnglingClubWebsite.Services;
using AnglingClubShared.DTOs;

namespace AnglingClubWebsite
{
    public partial class MainLayoutViewModel : ViewModelBase, IRecipient<TurnOnDebugMessages>, IRecipient<ShowConsoleMessage>, IRecipient<ShowProgress>, IRecipient<HideProgress>, IRecipient<ShowMessage>, IRecipient<LoggedIn>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly INavigationService _navigationService;

        public MainLayoutViewModel(
            IMessenger messenger, IAuthenticationService authenticationService, INavigationService navigationService = null) : base(messenger)
        {
            messenger.Register<TurnOnDebugMessages>(this);
            messenger.Register<LoggedIn>(this);
            messenger.Register<ShowConsoleMessage>(this);
            messenger.Register<ShowProgress>(this);
            messenger.Register<HideProgress>(this);
            messenger.Register<ShowMessage>(this);

            _authenticationService = authenticationService;

            defineStartupMenu();
            _navigationService = navigationService;
        }

        [ObservableProperty]
        private string _testMessage = "Hello from MainLayoutViewModel";

        [ObservableProperty]
        private List<MenuItem> _menu = new List<MenuItem>();

        [ObservableProperty]
        private bool _showDebugMessages = true;

        [ObservableProperty]
        private bool _showProgressBar = false;

        [ObservableProperty]
        private MessageSeverity _messageSeverity = MessageSeverity.Normal;

        [ObservableProperty]
        private string _messageTitle = "";

        [ObservableProperty]
        private string _messageBody = "";

        [ObservableProperty]
        private MessageButton? _confirmationButton;

        [ObservableProperty]
        private bool _messageVisible = false;

        public MemberDto? CurrentUser { get; set; }

        #region Message Handlers

        public void Receive(LoggedIn message)
        {
            CurrentUser = message.User;

            if (CurrentUser != null)
            {
                setupLoggedInMenu();

                if (CurrentUser.Admin)
                {
                    setupAdminMenu();
                }
            }
            else
            {
                setupLoggedOutMenu();
                _navigationService.NavigateTo("/", false);
            }
        }

        public void Receive(TurnOnDebugMessages message)
        {
            ShowDebugMessages = message.YesOrNo;
        }

        public void Receive(HideProgress message)
        {
            ShowProgressBar = false;
        }

        public void Receive(ShowProgress message)
        {
            ShowProgressBar = true;
        }

        public void Receive(ShowMessage message)
        {
            switch (message.State)
            {
                case MessageState.Info:
                    MessageSeverity = MessageSeverity.Info;
                    break;

                case MessageState.Error:
                    MessageSeverity = MessageSeverity.Error;
                    break;

                case MessageState.Warn:
                    MessageSeverity = MessageSeverity.Warning;
                    break;

                case MessageState.Success:
                    MessageSeverity = MessageSeverity.Success;
                    break;

                default:
                    break;
            }

            MessageTitle = message.Title;
            MessageBody = message.Body;
            ConfirmationButton = message.confirmationButtonDetails;
            MessageVisible = true;
        }

        public void Receive(ShowConsoleMessage message)
        {
            ShowConsoleMessage(message.Content);
        }

        #endregion Message Handlers

        #region Helper Methods

        public void ShowConsoleMessage(string message)
        {
            if (ShowDebugMessages)
            {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - {message}");
            }
        }

        public async Task OnConfirm()
        {
            await ConfirmationButton!.OnConfirmed!();
            MessageVisible = false;
        }

        public void defineStartupMenu()
        {
            setupBaseMenu();
            setupLoggedOutMenu();
        }

        public void setupBaseMenu()
        {
            Menu.Clear();

            Menu.Add(new MenuItem { Id = "01", Name = "Welcome", NavigateUrl = "/" });
            Menu.Add(new MenuItem { Id = "02", Name = "News", NavigateUrl = "/News" });
            Menu.Add(new MenuItem { Id = "03", Name = "Club Waters" });
            Menu.Add(new MenuItem { Id = "04", Name = "Matches" });
            Menu.Add(new MenuItem { Id = "05", Name = "Standings", HasSubMenu = true });
            Menu.Add(new MenuItem { Id = "05.1", ParentId = "05", Name = "Leagues" });
            Menu.Add(new MenuItem { Id = "05.2", ParentId = "05", Name = "Weights" });
            Menu.Add(new MenuItem { Id = "05.3", ParentId = "05", Name = "Trophies" });
            Menu.Add(new MenuItem { Id = "06", Name = "Diary of Events" });
            Menu.Add(new MenuItem { Id = "07", Name = "Buy Online", HasSubMenu = true });
            Menu.Add(new MenuItem { Id = "07.1", ParentId = "07", Name = "Memberships" });
            Menu.Add(new MenuItem { Id = "07.2", ParentId = "07", Name = "Day Tickets" });
            Menu.Add(new MenuItem { Id = "07.3", ParentId = "07", Name = "Guest Tickets" });
            Menu.Add(new MenuItem { Id = "08", Name = "Club Info", HasSubMenu = true });
            Menu.Add(new MenuItem { Id = "08.1", ParentId = "08", Name = "Club Officers" });
            Menu.Add(new MenuItem { Id = "08.2", ParentId = "08", Name = "Rules", HasSubMenu = true });
            Menu.Add(new MenuItem { Id = "08.2.1", ParentId = "08.2", Name = "General" });
            Menu.Add(new MenuItem { Id = "08.2.2", ParentId = "08.2", Name = "Match" });
            Menu.Add(new MenuItem { Id = "08.2.3", ParentId = "08.2", Name = "Junior General" });
            Menu.Add(new MenuItem { Id = "08.2.4", ParentId = "08.2", Name = "Junior Match" });
            Menu.Add(new MenuItem { Id = "08.3", ParentId = "08", Name = "Club Forms" });
            Menu.Add(new MenuItem { Id = "08.4", ParentId = "08", Name = "Privacy Notice" });
            Menu.Add(new MenuItem { Id = "08.5", ParentId = "08", Name = "Environmental" });
            Menu.Add(new MenuItem { Id = "08.6", ParentId = "08", Name = "Angling Trust" });
                                                             
        }

        public void setupLoggedOutMenu()
        {
            setupBaseMenu();

            Menu.Add(new MenuItem { Id = "11", Name = "Login", NavigateUrl = "/Login" });

            Menu = Menu.OrderBy(x => x.Id).ToList();
        }

        public void setupLoggedInMenu()
        {
            setupBaseMenu();

            Menu.Add(new MenuItem { Id = "10", Name = "My Details" });
            Menu.Add(new MenuItem { Id = "11", Name = "Logout", NavigateUrl = "/Logout" });

            Menu = Menu.OrderBy(x => x.Id).ToList();
        }

        public void setupAdminMenu()
        {
            Menu.Add(new MenuItem { Id = "09", Name = "Admin", HasSubMenu = true });
            Menu.Add(new MenuItem { Id = "09.1", ParentId = "09", Name = "Members" });
            Menu.Add(new MenuItem { Id = "09.2", ParentId = "09", Name = "User Admins" });
            Menu.Add(new MenuItem { Id = "09.3", ParentId = "09", Name = "Payments" });

            Menu = Menu.OrderBy(x => x.Id).ToList();
        }

        #endregion Helper Methods

        #region Events

        public override async Task Loaded()
        {
            await base.Loaded();
            CurrentUser = await _authenticationService.GetCurrentUser();
            if (CurrentUser != null)
            {
                setupLoggedInMenu();

                if (CurrentUser.Admin)
                {
                    setupAdminMenu();
                }
            }

        }

        #endregion
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
