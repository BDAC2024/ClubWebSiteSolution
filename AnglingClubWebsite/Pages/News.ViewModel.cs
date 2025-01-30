using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System;
using AnglingClubShared;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AnglingClubWebsite.Pages
{
    public partial class NewsViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;
        private readonly INewsService _newsService;

        public NewsViewModel(
            IAuthenticationService authenticationService,
            IMessenger messenger,
            ICurrentUserService currentUserService,
            INewsService newsService
            ) : base(messenger, currentUserService, authenticationService)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
            _newsService = newsService;

            //Items.Add(new NewsItem
            //{
            //    DbKey = "Item1",
            //    Date = DateTime.Now.AddDays(-1),
            //    Title = "News Item 1",
            //    Body = "A new <b>Test</b> message."
            //});
            //Items.Add(new NewsItem
            //{
            //    DbKey = "Item2",
            //    Date = DateTime.Now.AddDays(-2),
            //    Title = "News Item 2",
            //    Body = "Another <i>Test</i> message."
            //});
        }

        [ObservableProperty]
        private bool isUnlocked = false;

        [ObservableProperty]
        private ObservableCollection<NewsItem> items = new ObservableCollection<NewsItem>();

        public override async Task Loaded()
        {
            await getNews();

            await base.Loaded();
        }

        public void Unlock(bool unlock)
        {
            IsUnlocked = unlock;
        }

        private async Task getNews()
        {
            var items = await _newsService.ReadNews();

            if (items != null)
            {
                Items = new ObservableCollection<NewsItem>(items);
            }
        }

        public void AddNewsItem()
        {

        }

        public bool IsNew(DateTime itemDate)
        {
            var daysConsideredRecent = 14;
            var now = DateTime.Now;
            var newNewsDate = now.AddDays(daysConsideredRecent * -1);

            return itemDate > newNewsDate;
        }

        public async Task OnNewsItemDeleted(string itemId)
        {
            _messenger.Send<ShowMessage>(new ShowMessage(AnglingClubShared.Enums.MessageState.Info, $"Would be deleting: {itemId}", ""));
            await Task.Delay(0);
        }

        public async Task OnNewsItemEdited(string itemId)
        {
            _messenger.Send<ShowMessage>(new ShowMessage(AnglingClubShared.Enums.MessageState.Info, $"Would be editing: {itemId}", ""));
            await Task.Delay(0);
        }

    }
}
