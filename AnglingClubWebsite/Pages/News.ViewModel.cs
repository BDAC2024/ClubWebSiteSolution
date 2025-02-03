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
using CommunityToolkit.Mvvm.Input;
using Fishing.Client.Services;

namespace AnglingClubWebsite.Pages
{
    public partial class NewsViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;
        private readonly INewsService _newsService;
        private readonly ILogger<NewsViewModel> _logger;
        private readonly IAppDialogService _appDialogService;

        public NewsViewModel(
            IAuthenticationService authenticationService,
            IMessenger messenger,
            ICurrentUserService currentUserService,
            INewsService newsService,
            ILogger<NewsViewModel> logger,
            IAppDialogService appDialogService) : base(messenger, currentUserService, authenticationService)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
            _newsService = newsService;
            _logger = logger;
            _appDialogService = appDialogService;
        }

        [ObservableProperty]
        private bool isUnlocked = false;

        [ObservableProperty]
        private ObservableCollection<NewsItem> items = new ObservableCollection<NewsItem>();

        [ObservableProperty]
        private NewsItem? _newsItem = null;

        [ObservableProperty]
        private bool _isEditing = false;

        [ObservableProperty]
        private bool _isAdding = false;

        [ObservableProperty]
        private bool _submitting = false;

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
                    Unlock(false);
                    Unlock(true);
                }

                _messenger.Send(new HideProgress());
            }
        }

        public void AddNewsItem()
        {
            IsAdding = true;

            NewsItem = new NewsItem();
            NewsItem.Date = DateTime.Now;

        }

        [RelayCommand]
        private async Task Cancel()
        {
            IsAdding = false;
            IsEditing = false;

            await Task.Delay(0);
        }

        [RelayCommand(CanExecute = nameof(CanWeSave))]
        private async Task Save()
        {
            _messenger.Send<ShowProgress>();

            try
            {
                Submitting = true;
                await _newsService.SaveNewsItem(NewsItem);
                await getNews(true);

                IsAdding = false;
                IsEditing = false;
            }
            catch (Exception ex)
            {
                _appDialogService.SendMessage(MessageState.Error, "Save Failed", "Unable to save News item");
                _logger.LogError("Failed to save news", ex);
            }
            finally
            {
                Submitting = false;
                _messenger.Send<HideProgress>();
            }
        }

        public bool CanWeSave()
        {
            return true;
            //var valid = !(LoginModel.HasErrors || Submitting);
            //return valid;
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
                    "Cancel",
                    new MessageButton
                    {
                        Label = "Yes",
                        OnConfirmed = async () =>
                        {
                            _messenger.Send<ShowProgress>();

                            try
                            {
                                Submitting = true;

                                await _newsService.DeleteNewsItem(newsItem.DbKey);
                                await getNews(true);

                            }
                            catch (Exception ex)
                            {
                                _appDialogService.SendMessage(MessageState.Error, "Deletion Failed", "Unable to save News item");
                                _logger.LogError("Failed to delete news", ex);
                            }
                            finally
                            {
                                Submitting = false;
                                _messenger.Send<HideProgress>();
                            }

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
