using AnglingClubShared;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubWebsite.Models;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite.Pages
{
    public partial class News : RazorComponentBase
    {
        private readonly IMessenger _messenger;
        private readonly INewsService _newsService;
        private readonly ILogger<News> _logger;
        private readonly IDialogQueue _dialogQueue;

        public News(
            IAuthenticationService authenticationService,
            IMessenger messenger,
            ICurrentUserService currentUserService,
            INewsService newsService,
            ILogger<News> logger,
            IDialogQueue dialogQueue) : base(messenger, currentUserService, authenticationService)
        {
            _messenger = messenger;
            _newsService = newsService;
            _logger = logger;
            _dialogQueue = dialogQueue;
        }

        public bool IsUnlocked { get; set; }
        public List<NewsItem> Items { get; set; } = new();
        public NewsItem? NewsItem { get; set; }
        public bool IsAdding { get; set; }
        public bool DataLoaded { get; set; }
        public bool Submitting { get; set; }

        public override async Task Loaded()
        {
            await GetNews();
            IsUnlocked = false;
            await base.Loaded();
        }

        public void Unlock(bool unlock)
        {
            IsUnlocked = unlock;
        }

        private async Task GetNews(bool unlockAfterwards = false)
        {
            DataLoaded = false;

            try
            {
                Items = await _newsService.ReadNews() ?? new List<NewsItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load news");
            }
            finally
            {
                if (unlockAfterwards)
                {
                    IsUnlocked = true;
                }

                DataLoaded = true;
            }
        }

        public void AddNewsItem()
        {
            IsAdding = true;
            NewsItem = new NewsItem
            {
                Date = DateTime.Now
            };
        }

        public async Task Cancel()
        {
            IsAdding = false;
            NewsItem = null;

            await GetNews(true);
            StateHasChanged();
        }

        public async Task Save()
        {
            DataLoaded = false;

            try
            {
                Submitting = true;
                await _newsService.SaveNewsItem(NewsItem!);
                await GetNews(true);
                IsAdding = false;
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
                StateHasChanged();
            }
        }

        public bool IsNew(DateTime itemDate)
        {
            const int daysConsideredRecent = 14;
            var newNewsDate = DateTime.Now.AddDays(-daysConsideredRecent);

            return itemDate > newNewsDate;
        }

        public void OnNewsItemDeleted(NewsItem newsItem)
        {
            _dialogQueue.Enqueue(new DialogRequest
            {
                Kind = DialogKind.Confirm,
                Severity = DialogSeverity.Warn,
                Title = "Please confirm",
                Message = $"Do you really want to delete the news item '{newsItem.Title}'?",
                CancelText = "Cancel",
                ConfirmText = "Yes",
                OnConfirmAsync = async () => await DeleteNewsItem(newsItem)
            });
        }

        private async Task DeleteNewsItem(NewsItem newsItem)
        {
            DataLoaded = false;

            try
            {
                Submitting = true;
                await _newsService.DeleteNewsItem(newsItem.DbKey);
                await GetNews(true);
            }
            catch (Exception ex)
            {
                _messenger.Send(new ShowMessage(MessageState.Error, "Deletion Failed", "Unable to delete News item"));
                _logger.LogError(ex, "Failed to delete news");
            }
            finally
            {
                Submitting = false;
                DataLoaded = true;
                StateHasChanged();
            }
        }

        public void OnNewsItemEdited(string itemId)
        {
            NewsItem = Items.FirstOrDefault(item => item.DbKey == itemId);
        }
    }
}
