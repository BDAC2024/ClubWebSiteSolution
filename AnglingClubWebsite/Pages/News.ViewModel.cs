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
using AnglingClubShared.Enums;

namespace AnglingClubWebsite.Pages
{
    public partial class NewsViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;
        private readonly INewsService _newsService;
        private readonly ILogger<NewsViewModel> _logger;

        public NewsViewModel(
            IAuthenticationService authenticationService,
            IMessenger messenger,
            ICurrentUserService currentUserService,
            INewsService newsService,
            ILogger<NewsViewModel> logger) : base(messenger, currentUserService, authenticationService)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
            _newsService = newsService;
            _logger = logger;
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

        private async Task getNews(bool unlockAfterwards = false)
        {
            _messenger.Send(new ShowProgress());

            if (unlockAfterwards)
            {
                Unlock(false);
            }

            try
            {
                var items = await _newsService.ReadNews();

                if (items != null)
                {
                    Items = new ObservableCollection<NewsItem>(items);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"getNews: {ex.Message}");
            }
            finally
            {
                if (unlockAfterwards)
                {
                    Unlock(true);
                }

                _messenger.Send(new HideProgress());
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

        public async Task OnNewsItemDeleted(NewsItem newsItem)
        {
            _messenger.Send<ShowMessage>(
                new ShowMessage
                (
                    MessageState.Info,
                    "Please confirm",
                    $"Do you really want to delete the news item '{newsItem.Title}'?",
                    new MessageButton
                    {
                        Label = "Yes",
                        OnConfirmed = async () =>
                        {
                            _messenger.Send<ShowProgress>();

                            await _newsService.DeleteNewsItem(newsItem.DbKey);
                            await getNews(true);

                            _messenger.Send<HideProgress>();
                        }
                    }
                )
            );

            await Task.Delay(0);
        }

        public async Task OnNewsItemEdited(string itemId)
        {
            _messenger.Send<ShowMessage>(new ShowMessage(AnglingClubShared.Enums.MessageState.Info, $"Would be editing: {itemId}", ""));
            await Task.Delay(0);
        }


    }
}
