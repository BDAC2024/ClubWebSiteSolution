using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System;
using AnglingClubShared;

namespace AnglingClubWebsite.Pages
{
    public partial class NewsViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;

        public NewsViewModel(
            IAuthenticationService authenticationService,
            IMessenger messenger,
            ICurrentUserService currentUserService) : base(messenger, currentUserService)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;

            Items.Add(new NewsItem
            {
                DbKey = "Item1",
                Date = DateTime.Now.AddDays(-1),
                Title = "News Item 1",
                Body = "A new <b>Test</b> message."
            });
            Items.Add(new NewsItem
            {
                DbKey = "Item2",
                Date = DateTime.Now.AddDays(-2),
                Title = "News Item 2",
                Body = "Another <i>Test</i> message."
            });
        }

        [ObservableProperty]
        private bool isUnlocked = false;

        [ObservableProperty]
        private ObservableCollection<NewsItem> items = new ObservableCollection<NewsItem>();

        public void Unlock(bool unlock)
        {
            IsUnlocked = unlock;
        }

        public void AddNewsItem()
        {

        }

        public bool IsNew(DateTime itemDate)
        {
            return true;
        }

        public async Task OnNewsItemDeleted(string itemId)
        {
            _messenger.Send<ShowMessage>(new ShowMessage(AnglingClubShared.Enums.MessageState.Info, $"Would be deleting: {itemId}", ""));
        }

        public async Task OnNewsItemEdited(string itemId)
        {
            _messenger.Send<ShowMessage>(new ShowMessage(AnglingClubShared.Enums.MessageState.Info, $"Would be editing: {itemId}", ""));
        }

    }
}
