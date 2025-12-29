using AnglingClubShared;
using AnglingClubWebsite.SharedComponents;
using AnglingClubShared.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Syncfusion.Blazor.Notifications;
using AnglingClubShared.Entities;
using AnglingClubWebsite.Services;
using AnglingClubShared.DTOs;
using Syncfusion.Blazor.Lists;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Syncfusion.Blazor.Navigations;
using Microsoft.AspNetCore.Components;

namespace AnglingClubWebsite
{
    public partial class MainLayoutViewModel : ViewModelBase, 
        IRecipient<BrowserChange>,
        IRecipient<TurnOnDebugMessages>, 
        IRecipient<ShowConsoleMessage>, 
        IRecipient<ShowProgress>, 
        IRecipient<HideProgress>, 
        IRecipient<ShowMessage>, 
        IRecipient<LoggedIn>,
        IRecipient<SelectMenuItem>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly INavigationService _navigationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly BrowserService _browserService;
        private readonly IMessenger _messsenger;
        private readonly IConfiguration _configuration;

        public MainLayoutViewModel(
            IMessenger messenger,
            IAuthenticationService authenticationService,
            INavigationService navigationService,
            ICurrentUserService currentUserService,
            BrowserService browserService,
            IConfiguration configuration) : base(messenger, currentUserService, authenticationService)
        {
            messenger.Register<TurnOnDebugMessages>(this);
            messenger.Register<LoggedIn>(this);
            messenger.Register<ShowConsoleMessage>(this);
            messenger.Register<ShowProgress>(this);
            messenger.Register<HideProgress>(this);
            messenger.Register<ShowMessage>(this);
            messenger.Register<SelectMenuItem>(this);
            messenger.Register<BrowserChange>(this);

            _authenticationService = authenticationService;
            _configuration = configuration;

            defineStartupMenu();
            _navigationService = navigationService;
            _currentUserService = currentUserService;
            _browserService = browserService;
            _messsenger = messenger;
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
        private string _messageCloseButtonTitle = "";

        [ObservableProperty]
        private MessageButton? _confirmationButton;

        [ObservableProperty]
        private bool _messageVisible = false;

        [ObservableProperty]
        private string[] _selectedItems = new string[] { "01" };

        [ObservableProperty]
        private string[] _expandedNodes = new string[0];

        [ObservableProperty]
        private string _browserDevice = "UNKNOWN";

        [ObservableProperty]
        private bool _browserPortrait = false;

        [ObservableProperty]
        private DeviceSize _browserSize = DeviceSize.Unknown;

        [ObservableProperty]
        private int _browserWidth = 0;

        [ObservableProperty]
        private int _browserHeight = 0;

        #region Message Handlers

        public void Receive(SelectMenuItem message)
        {
            SelectMenuItem(message.NavigateUrl);
            ShowConsoleMessage($"SelectMenuItem: About to navigate to {message.NavigateUrl}");
            _navigationService.NavigateTo(message.NavigateUrl, false);
        }

        public void Receive(LoggedIn message)
        {
            _currentUserService.User = message.User;

            if (!string.IsNullOrEmpty(_currentUserService.User.Id))
            {
                setupLoggedInMenu();

                if (_currentUserService.User.Admin)
                {
                    setupAdminMenu();
                }
            }
            else
            {
                setupLoggedOutMenu();
                SelectMenuItem("/");
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
            MessageCloseButtonTitle = message.CloseButtonTitle!;
            ConfirmationButton = message.confirmationButtonDetails;
            MessageVisible = true;
        }

        public void Receive(ShowConsoleMessage message)
        {
            ShowConsoleMessage(message.Content);
        }

        public void Receive(BrowserChange message)
        {
            BrowserPortrait = _browserService.IsPortrait;
            BrowserSize = _browserService.DeviceSize;
            BrowserWidth = _browserService.Dimensions.Width;
            BrowserHeight = _browserService.Dimensions.Height;
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
            ShowConsoleMessage($"setupBaseMenu");

            List<MenuItem> menuItems = new List<MenuItem>();

            menuItems.Add(new MenuItem { Id = "01", Name = "Welcome", NavigateUrl = menuUrl("/")});
            menuItems.Add(new MenuItem { Id = "02", Name = "News", NavigateUrl = menuUrl("/News") });
            menuItems.Add(new MenuItem { Id = "03", Name = "Club Waters", NavigateUrl = menuUrl("/Waters") });
            menuItems.Add(new MenuItem { Id = "04", Name = "Matches", NavigateUrl = menuUrl("/Matches") });
            menuItems.Add(new MenuItem { Id = "045", Name = "Junior Open Matches", NavigateUrl = menuUrl("/register") });
            menuItems.Add(new MenuItem { Id = "05", Name = "Standings", HasSubMenu = true });
            menuItems.Add(new MenuItem { Id = "05.1", ParentId = "05", Name = "Leagues" });
            menuItems.Add(new MenuItem { Id = "05.2", ParentId = "05", Name = "Weights" });
            menuItems.Add(new MenuItem { Id = "05.3", ParentId = "05", Name = "Trophies" });
            menuItems.Add(new MenuItem { Id = "06", Name = "Diary of Events", NavigateUrl = menuUrl("/diary") });
            menuItems.Add(new MenuItem { Id = "07", Name = "Buy Online", HasSubMenu = true, IsNew = false });
            menuItems.Add(new MenuItem { Id = "07.1", ParentId = "07", Name = "Memberships", NavigateUrl = menuUrl("/buyMemberships"), IsNew = false });
            menuItems.Add(new MenuItem { Id = "07.2", ParentId = "07", Name = "Day Tickets", NavigateUrl = menuUrl("/buyDayTickets"), IsNew = false });
            menuItems.Add(new MenuItem { Id = "08", Name = "Club Info", HasSubMenu = true });
            menuItems.Add(new MenuItem { Id = "08.1", ParentId = "08", Name = "Club Officers" });
            menuItems.Add(new MenuItem { Id = "08.2", ParentId = "08", Name = "Rules", HasSubMenu = true });
            menuItems.Add(new MenuItem { Id = "08.2.1", ParentId = "08.2", Name = "General" });
            menuItems.Add(new MenuItem { Id = "08.2.2", ParentId = "08.2", Name = "Match", NavigateUrl = menuUrl("/rulesMatch") });
            menuItems.Add(new MenuItem { Id = "08.2.3", ParentId = "08.2", Name = "Junior General" });
            menuItems.Add(new MenuItem { Id = "08.2.4", ParentId = "08.2", Name = "Junior Match" });
            menuItems.Add(new MenuItem { Id = "08.3", ParentId = "08", Name = "Club Forms", NavigateUrl = menuUrl("/forms") });
            menuItems.Add(new MenuItem { Id = "08.4", ParentId = "08", Name = "Privacy Notice" });
            menuItems.Add(new MenuItem { Id = "08.5", ParentId = "08", Name = "Environmental" });
            menuItems.Add(new MenuItem { Id = "08.6", ParentId = "08", Name = "Angling Trust" });

            Menu.Clear();
            Menu.AddRange(menuItems);
        }

        public void setupLoggedOutMenu()
        {
            setupBaseMenu();

            List<MenuItem> menuItems = new List<MenuItem>();

            menuItems.Add(new MenuItem { Id = "11", Name = "Login", NavigateUrl = menuUrl("/Login") });

            Menu.AddRange(menuItems);
            Menu = Menu.OrderBy(x => x.Id).ToList();
        }

        public void setupLoggedInMenu()
        {
            ShowConsoleMessage($"setupLoggedInMenu");

            setupBaseMenu();

            List<MenuItem> menuItems = new List<MenuItem>();

            menuItems.Add(new MenuItem { Id = "07.3", ParentId = "07", Name = "Guest Tickets", NavigateUrl = menuUrl("/buyGuestTickets"), IsNew = false });
            menuItems.Add(new MenuItem { Id = "10", Name = "My Details" });
            menuItems.Add(new MenuItem { Id = "11", Name = "Logout", NavigateUrl = menuUrl("/Logout") });

            Menu.AddRange(menuItems);
            Menu = Menu.OrderBy(x => x.Id).ToList();
        }

        public void setupAdminMenu()
        {
            ShowConsoleMessage($"setupAdminMenu");

            List<MenuItem> menuItems = new List<MenuItem>();

            menuItems.Add(new MenuItem { Id = "09", Name = "Admin", HasSubMenu = true });
            menuItems.Add(new MenuItem { Id = "09.1", ParentId = "09", Name = "Members" });
            menuItems.Add(new MenuItem { Id = "09.2", ParentId = "09", Name = "User Admins" });
            menuItems.Add(new MenuItem { Id = "09.3", ParentId = "09", Name = "Payments" });

            Menu.AddRange(menuItems);
            Menu = Menu.OrderBy(x => x.Id).ToList();
        }

        public void SelectMenuItem(string navigateUrl)
        {
            var menuItem = Menu.FirstOrDefault(x => x.NavigateUrl != null && (x.NavigateUrl.ToLower() == navigateUrl.ToLower()));

            if (menuItem != null)
            {
                List<string> parents = new List<string>();

                var selectedItemId = menuItem.Id;

                var parentId = menuItem.ParentId;
                var parentItem = Menu.FirstOrDefault(x => x.Id == parentId);

                while (parentItem != null)
                {
                    parents.Add(parentItem.Id);

                    parentId = parentItem.ParentId;
                    parentItem = Menu.FirstOrDefault(x => x.Id == parentId);
                }

                if (parents.Any())
                {
                    ExpandedNodes = parents.OrderBy(x => x).ToArray();
                }

                SelectedItems = new string[] { selectedItemId };

            }
            else
            {
                ShowConsoleMessage($"SelectMenuItem - item {navigateUrl} not found!");
            }
        }

        private string menuUrl(string item)
        {
            return _configuration["BaseHref"] + item;
        }

        #endregion Helper Methods

        #region Events

        public override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        public override async Task Loaded()
        {
            if (!string.IsNullOrEmpty(_currentUserService.User.Id))
            {
                setupLoggedInMenu();

                if (_currentUserService.User.Admin)
                {
                    setupAdminMenu();
                }
            }

            await base.Loaded();
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
        public bool IsNew { get; set; } = false;
    }

    #endregion Helper classes

}
