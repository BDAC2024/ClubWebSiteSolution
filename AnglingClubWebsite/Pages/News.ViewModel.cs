using AnglingClubShared.Entities;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using AnglingClubShared;
using AnglingClubShared.Enums;
using CommunityToolkit.Mvvm.Input;
using AnglingClubWebsite.Models;

namespace AnglingClubWebsite.Pages
{
    public partial class NewsViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;
        private readonly INewsService _newsService;
        private readonly ILogger<NewsViewModel> _logger;
        private readonly IDialogQueue _dialogQueue;

        public NewsViewModel(
            IAuthenticationService authenticationService,
            IMessenger messenger,
            ICurrentUserService currentUserService,
            INewsService newsService,
            ILogger<NewsViewModel> logger,
            IDialogQueue dialogQueue) : base(messenger, currentUserService, authenticationService)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
            _newsService = newsService;
            _logger = logger;
            _dialogQueue = dialogQueue;
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
        private bool _dataLoaded = false;

        [ObservableProperty]
        private bool _submitting = false;

        public override async Task Loaded()
        {
            await getNews();
            IsUnlocked = false;
            await base.Loaded();
        }

        public void Unlock(bool unlock)
        {
            IsUnlocked = unlock;
        }

        private async Task getNews(bool unlockAfterwards = false)
        {
            DataLoaded = false;

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

                DataLoaded = true;
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
            NewsItem = null;

            await getNews(true);
            await Task.Delay(0);
        }

        [RelayCommand(CanExecute = nameof(CanWeSave))]
        private async Task Save()
        {
            DataLoaded = false;

            try
            {
                Submitting = true;
                await _newsService.SaveNewsItem(NewsItem!);
                await getNews(true);

                IsAdding = false;
                IsEditing = false;
            }
            catch (Exception ex)
            {
                _dialogQueue.Enqueue(new DialogRequest
                {
                    Kind = DialogKind.Alert,
                    Severity = DialogSeverity.Error,
                    Title = "Save Failed",
                    Message = "Unable to save News item"
                });

                _logger.LogError(ex, "Failed to save news");
            }
            finally
            {
                Submitting = false;
                NewsItem = null;
                DataLoaded = true;
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
            _dialogQueue.Enqueue(new DialogRequest
            {
                Kind = DialogKind.Confirm,
                Severity = DialogSeverity.Warn,
                Title = "Please confirm",
                Message = $"Do you really want to delete the news item '{newsItem.Title}'?",
                CancelText = "Cancel",
                ConfirmText = "Yes",
                OnConfirmAsync = async () =>
                {
                    DataLoaded = false;

                    try
                    {
                        Submitting = true;

                        await _newsService.DeleteNewsItem(newsItem.DbKey);
                        await getNews(true);
                    }
                    catch (Exception ex)
                    {
                        _messenger.Send<ShowMessage>(new ShowMessage(MessageState.Error, "Deletion Failed", "Unable to delete News item"));
                        _logger.LogError(ex, "Failed to delete news");
                    }
                    finally
                    {
                        Submitting = false;
                        DataLoaded = true;
                    }
                }
            });
        }

        public async Task OnNewsItemEdited(string itemId)
        {
            NewsItem = Items.FirstOrDefault(i => i.DbKey == itemId);
            await Task.Delay(0);
        }


    }
}
