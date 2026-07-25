using AnglingClubShared.Enums;
using AnglingClubWebsite.Models;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Navigations;

namespace AnglingClubWebsite
{
    public partial class MainLayout : LayoutComponentBase,
        IRecipient<BrowserChange>,
        IRecipient<LoggedIn>,
        IRecipient<SelectMenuItem>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly INavigationService _navigationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly BrowserService _browserService;
        private readonly IMessenger _messenger;
        private readonly IConfiguration _configuration;
        private readonly NavigationManager _navigationManager;

        public MainLayout(
            IMessenger messenger,
            IAuthenticationService authenticationService,
            INavigationService navigationService,
            ICurrentUserService currentUserService,
            BrowserService browserService,
            IConfiguration configuration,
            NavigationManager navigationManager)
        {
            messenger.Register<LoggedIn>(this);
            messenger.Register<SelectMenuItem>(this);
            messenger.Register<BrowserChange>(this);

            _authenticationService = authenticationService;
            _configuration = configuration;

            _navigationService = navigationService;
            _currentUserService = currentUserService;
            _browserService = browserService;
            _messenger = messenger;
            _navigationManager = navigationManager;

            defineStartupMenu();
            setBrowserDetails();
        }

        public List<MenuItem> Menu { get; set; } = new();


        public string[] SelectedItems { get; set; } = ["01"];

        public string[] ExpandedNodes { get; set; } = [];

        public bool BrowserPortrait { get; set; }

        public DeviceSize BrowserSize { get; set; } = DeviceSize.Unknown;

        public int BrowserWidth { get; set; }

        public int BrowserHeight { get; set; }

        #region Message Handlers

        public void Receive(SelectMenuItem message)
        {
            SelectMenuItem(message.NavigateUrl);
            _messenger.Send<ShowConsoleMessage>(new ShowConsoleMessage($"SelectMenuItem: About to navigate to {message.NavigateUrl}"));
            _navigationService.NavigateTo(message.NavigateUrl, false);
            _ = InvokeAsync(StateHasChanged);
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

                if (_currentUserService.User.CommitteeMember)
                {
                    setupCommitteeMenu();
                }


                if (_currentUserService.User.Developer)
                {
                    setupDeveloperMenu();
                }

            }
            else
            {
                setupLoggedOutMenu();
                if (message.GotoLoginIfLoggedOut)
                {
                    SelectMenuItem("/login");
                    _navigationService.NavigateTo("/login", false);
                }
                else
                {
                    SelectMenuItem("/");
                    _navigationService.NavigateTo("/", false);
                }
            }

            _ = InvokeAsync(StateHasChanged);
        }



        public void Receive(BrowserChange message)
        {
            setBrowserDetails();
            _ = InvokeAsync(StateHasChanged);
        }

        #endregion Message Handlers

        #region Helper Methods

        public void ShowConsoleMessage(string msg)
        {
            _messenger.Send<ShowConsoleMessage>(new ShowConsoleMessage(msg));
        }

        public async Task OnConfirm()
        {
            //await ConfirmationButton!.OnConfirmed!();
            //MessageVisible = false;
        }

        public void defineStartupMenu()
        {
            setupBaseMenu();
            setupLoggedOutMenu();
        }

        public void setupBaseMenu()
        {
            _messenger.Send<ShowConsoleMessage>(new ShowConsoleMessage($"setupBaseMenu"));

            List<MenuItem> menuItems = new List<MenuItem>();

            menuItems.Add(new MenuItem { Id = "01", Name = "Welcome", NavigateUrl = menuUrl("/") });
            menuItems.Add(new MenuItem { Id = "02", Name = "News", NavigateUrl = menuUrl("/News") });
            menuItems.Add(new MenuItem { Id = "03", Name = "Club Waters", NavigateUrl = menuUrl("/Waters") });
            menuItems.Add(new MenuItem { Id = "03.1", Name = "Reserved Pegs", IsNew = true, NavigateUrl = menuUrl("/PegReservations") });
            menuItems.Add(new MenuItem { Id = "04", Name = "Matches", NavigateUrl = menuUrl("/Matches") });
            menuItems.Add(new MenuItem { Id = "045", Name = "Junior Open Matches", NavigateUrl = menuUrl("/JuniorOpenMatches") });
            menuItems.Add(new MenuItem { Id = "05", Name = "Standings", HasSubMenu = true });
            menuItems.Add(new MenuItem { Id = "05.1", ParentId = "05", Name = "Leagues", NavigateUrl = menuUrl("/StandingsLeague") });
            menuItems.Add(new MenuItem { Id = "05.2", ParentId = "05", Name = "Weights", NavigateUrl = menuUrl("/StandingsWeights") });
            menuItems.Add(new MenuItem { Id = "05.3", ParentId = "05", Name = "Trophies", NavigateUrl = menuUrl("/StandingsTrophies") });
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
            _messenger.Send<ShowConsoleMessage>(new ShowConsoleMessage($"setupLoggedInMenu"));
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
            _messenger.Send<ShowConsoleMessage>(new ShowConsoleMessage($"setupAdminMenu"));

            List<MenuItem> menuItems = new List<MenuItem>();

            menuItems.Add(new MenuItem { Id = "09", Name = "Admin", HasSubMenu = true });
            menuItems.Add(new MenuItem { Id = "09.1", ParentId = "09", Name = "Members" });
            menuItems.Add(new MenuItem { Id = "09.2", ParentId = "09", Name = "Documentation", NavigateUrl = menuUrl("/Documentation") });
            menuItems.Add(new MenuItem { Id = "09.3", ParentId = "09", Name = "User Admins" });
            menuItems.Add(new MenuItem { Id = "09.4", ParentId = "09", Name = "Book Printing", NavigateUrl = menuUrl("/BookPrinting") });
            menuItems.Add(new MenuItem { Id = "09.5", ParentId = "09", Name = "Payments" });
            menuItems.Add(new MenuItem { Id = "09.6", ParentId = "09", Name = "Junior Open Registrations", NavigateUrl = menuUrl("/JuniorOpenMatches/1") });

            Menu.AddRange(menuItems);
            Menu = Menu.OrderBy(x => x.Id).ToList();
        }

        public void setupDeveloperMenu()
        {
            _messenger.Send<ShowConsoleMessage>(new ShowConsoleMessage($"setupDeveloperMenu"));

            List<MenuItem> menuItems = new List<MenuItem>();

            menuItems.Add(new MenuItem { Id = "105", Name = "About", NavigateUrl = menuUrl("/About") });

            Menu.AddRange(menuItems);
            Menu = Menu.OrderBy(x => x.Id).ToList();
        }

        public void setupCommitteeMenu()
        {
            _messenger.Send<ShowConsoleMessage>(new ShowConsoleMessage($"setupCommitteeMenu"));

            List<MenuItem> menuItems = new List<MenuItem>();

            menuItems.Add(new MenuItem { Id = "075", Name = "Meetings", HasSubMenu = true });
            menuItems.Add(new MenuItem { Id = "075.1", ParentId = "075", Name = "Minutes", NavigateUrl = menuUrl("/MeetingMinutes") });

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
                _messenger.Send<ShowConsoleMessage>(new ShowConsoleMessage($"SelectMenuItem - item {navigateUrl} not found!"));
            }
        }

        private string menuUrl(string item)
        {
            return _configuration["BaseHref"] + item;
        }

        private void setBrowserDetails()
        {
            BrowserPortrait = _browserService.IsPortrait;
            BrowserSize = _browserService.DeviceSize;
            BrowserWidth = _browserService.Dimensions.Width;
            BrowserHeight = _browserService.Dimensions.Height;
        }

        #endregion Helper Methods

        #region Events

        protected override void OnInitialized()
        {
            var pageRoute = _navigationManager.Uri.Replace(_navigationManager.BaseUri, "");

            if (!string.IsNullOrEmpty(pageRoute))
            {
                ShowConsoleMessage($"pageRoute: {pageRoute}");
                SelectMenuItem($"/{pageRoute}");
            }

            base.OnInitialized();
        }

        protected override async Task OnInitializedAsync()
        {
            _currentUserService.User = await _authenticationService.GetCurrentUser();

            if (!string.IsNullOrEmpty(_currentUserService.User.Id))
            {
                setupLoggedInMenu();

                if (_currentUserService.User.Admin)
                {
                    setupAdminMenu();
                }

                if (_currentUserService.User.CommitteeMember)
                {
                    setupCommitteeMenu();
                }

                if (_currentUserService.User.Developer)
                {
                    setupDeveloperMenu();
                }
            }

            await base.OnInitializedAsync();
        }

        public bool debuggingDeviceType = false;
        public string SidebarTarget = ".menu-and-main-wrapper";
        public bool menuShowingOnMobile = false;
        public bool menuIsOpen = false;
        public bool oldMenuIsOpen = false;

        public void HideOnClick()
        {
            if (!menuShowingOnMobile)
            {
                return;
            }

            ShowConsoleMessage("HideOnClick: Menu closing");
            menuIsOpen = false;
            menuShowingOnMobile = false;
        }

        public void ShowMenu()
        {
            ShowConsoleMessage($"Toggle:oldMenuIsOpen - {oldMenuIsOpen}");
            ShowConsoleMessage($"Toggle:mobile - {BrowserSize == DeviceSize.Small}");
            menuIsOpen = !oldMenuIsOpen;
            menuShowingOnMobile = BrowserSize == DeviceSize.Small && menuIsOpen;
        }

        public void OnMenuOpen(Syncfusion.Blazor.Navigations.EventArgs args)
        {
            oldMenuIsOpen = true;
            ShowConsoleMessage("Menu opening");
        }

        public void OnMenuClose(Syncfusion.Blazor.Navigations.EventArgs args)
        {
            oldMenuIsOpen = false;
            ShowConsoleMessage("Menu closing");
        }

        public void MenuSelected(NodeClickEventArgs args)
        {
            bool isLeafNode = !args.NodeData.HasChildren;
            ShowConsoleMessage($"MenuSelected - leaf node {isLeafNode}");

            if (BrowserSize == DeviceSize.Small && isLeafNode)
            {
                menuIsOpen = false;
                menuShowingOnMobile = false;
            }
        }

        #endregion Events
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
