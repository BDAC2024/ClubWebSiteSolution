using AnglingClubShared.DTOs;
using AnglingClubWebsite.Helpers;
using AnglingClubWebsite.Models;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Navigations;

namespace AnglingClubWebsite.Pages
{
    public partial class Documentation : RazorComponentBase
    {
        private readonly IDocumentService _documentService;
        private readonly IDialogQueue _dialogQueue;
        private readonly IMessenger _messenger;
        private readonly INavigationService _navigationService;

        public Documentation(
            ICurrentUserService currentUserService,
            IAuthenticationService authenticationService,
            IMessenger messenger,
            IDocumentService documentService,
            IDialogQueue dialogQueue,
            INavigationService navigationService) : base(messenger, currentUserService, authenticationService)
        {
            _documentService = documentService;
            _dialogQueue = dialogQueue;
            _messenger = messenger;
            _navigationService = navigationService;
        }

        private List<DocumentationBucketItemDto> BucketItems { get; set; } = new();
        public List<DocumentationTreeNode> TreeNodes { get; set; } = new();
        public List<DocumentationFileItem> FilesInSelectedFolder { get; set; } = new();

        public string[] SelectedTreeNodes { get; set; } = new[] { RootFolderId };
        public string SelectedFolderPath { get; set; } = string.Empty;
        public string NewFolderName { get; set; } = string.Empty;

        public bool DataLoaded { get; set; }

        private const string RootFolderId = "__root__";

        public string SelectedFolderLabel => string.IsNullOrWhiteSpace(SelectedFolderPath) ? "Root" : SelectedFolderPath;

        public override async Task Loaded()
        {
            await RefreshAsync();
            await base.Loaded();
        }

        private async Task RefreshAsync()
        {
            DataLoaded = false;

            var response = await _documentService.GetDocumentationItems();
            BucketItems = response?.Items ?? new List<DocumentationBucketItemDto>();

            BuildTree();
            RefreshFilesForSelectedFolder();

            DataLoaded = true;
            StateHasChanged();
        }

        private void BuildTree()
        {
            var folderSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                string.Empty
            };

            foreach (var item in BucketItems)
            {
                var key = item.Key.Trim('/');
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                var working = item.IsFolderPlaceholder ? key : Path.GetDirectoryName(key)?.Replace("\\", "/", StringComparison.Ordinal) ?? string.Empty;

                while (!string.IsNullOrWhiteSpace(working))
                {
                    folderSet.Add(working);
                    working = Path.GetDirectoryName(working)?.Replace("\\", "/", StringComparison.Ordinal) ?? string.Empty;
                }
            }

            var nodes = new List<DocumentationTreeNode>
            {
                new DocumentationTreeNode
                {
                    Id = RootFolderId,
                    ParentId = null,
                    Name = "Root",
                    IconCss = "fa-solid fa-folder-tree",
                    Expanded = true,
                    HasChildren = true
                }
            };

            foreach (var folder in folderSet.Where(x => !string.IsNullOrWhiteSpace(x)).OrderBy(x => x))
            {
                var parent = Path.GetDirectoryName(folder)?.Replace("\\", "/", StringComparison.Ordinal) ?? string.Empty;
                nodes.Add(new DocumentationTreeNode
                {
                    Id = folder,
                    ParentId = string.IsNullOrWhiteSpace(parent) ? RootFolderId : parent,
                    Name = Path.GetFileName(folder),
                    IconCss = "fa-solid fa-folder",
                    Expanded = false,
                    HasChildren = folderSet.Any(x => !string.Equals(x, folder, StringComparison.OrdinalIgnoreCase)
                                                    && x.StartsWith($"{folder}/", StringComparison.OrdinalIgnoreCase))
                });
            }

            TreeNodes = nodes;

            if (!TreeNodes.Any(x => x.Id == SelectedFolderPath))
            {
                SelectedFolderPath = string.Empty;
                SelectedTreeNodes = new[] { RootFolderId };
            }
        }

        private void RefreshFilesForSelectedFolder()
        {
            var prefix = string.IsNullOrWhiteSpace(SelectedFolderPath) ? string.Empty : $"{SelectedFolderPath.TrimEnd('/')}/";

            FilesInSelectedFolder = BucketItems
                .Where(x => !x.IsFolderPlaceholder)
                .Where(x => x.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Where(x =>
                {
                    var remainder = x.Key.Substring(prefix.Length);
                    return !string.IsNullOrWhiteSpace(remainder) && !remainder.Contains('/');
                })
                .Select(x => new DocumentationFileItem
                {
                    Key = x.Key,
                    FileName = Path.GetFileName(x.Key),
                    CreatedUtc = x.LastModifiedUtc
                })
                .OrderByDescending(x => x.CreatedUtc)
                .ToList();
        }

        private Task OnFolderClicked(NodeClickEventArgs args)
        {
            if (args.NodeData is null)
            {
                return Task.CompletedTask;
            }

            SelectedFolderPath = args.NodeData.Id == RootFolderId ? string.Empty : args.NodeData.Id;
            SelectedTreeNodes = new[] { args.NodeData.Id };
            RefreshFilesForSelectedFolder();
            return Task.CompletedTask;
        }

        private async Task CreateFolderAsync()
        {
            var folderName = NewFolderName.Trim();
            if (string.IsNullOrWhiteSpace(folderName))
            {
                return;
            }

            var safeFolderName = folderName.Replace("\\", "/", StringComparison.Ordinal).Trim('/');
            if (safeFolderName.Contains("/"))
            {
                _messenger.Send(new ShowMessage(MessageState.Warn, "Invalid folder name", "Folder name must not contain path separators."));
                return;
            }

            var targetPath = string.IsNullOrWhiteSpace(SelectedFolderPath)
                ? safeFolderName
                : $"{SelectedFolderPath.TrimEnd('/')}/{safeFolderName}";

            try
            {
                await _documentService.CreateDocumentationFolder(targetPath);
                _messenger.Send(new ShowToast(MessageState.Success, "Folder created"));
                NewFolderName = string.Empty;
                await RefreshAsync();
            }
            catch (ApiValidationException ex)
            {
                _messenger.Send(new ShowMessage(MessageState.Warn, "Create folder failed", ex.Message));
            }
            catch (Exception)
            {
                _messenger.Send(new ShowMessage(MessageState.Error, "Create folder failed", "Unable to create folder."));
            }
        }

        private async Task UploadHandler(UploadChangeEventArgs args)
        {
            if (!args.Files.Any())
            {
                return;
            }

            var selectedFile = args.Files.First();

            try
            {
                await UploadWithOverwriteOption(selectedFile, false);
            }
            catch (ApiValidationException ex)
            {
                _messenger.Send(new ShowMessage(MessageState.Warn, "Upload failed", ex.Message));
            }
            catch (S3UploadException ex)
            {
                _messenger.Send(new ShowMessage(MessageState.Error, "Upload failed", ex.UserMessage));
            }
            catch (Exception)
            {
                _messenger.Send(new ShowMessage(MessageState.Error, "Upload failed", "Unable to upload file."));
            }
        }

        private async Task UploadWithOverwriteOption(UploadFiles selectedFile, bool overwrite)
        {
            var uploadDetails = await _documentService.GetDocumentationUploadUrl(SelectedFolderPath, selectedFile, overwrite);
            if (uploadDetails == null)
            {
                throw new Exception("Unable to get upload url");
            }

            if (uploadDetails.FileAlreadyExists)
            {
                _dialogQueue.Enqueue(new DialogRequest
                {
                    Kind = DialogKind.Confirm,
                    Severity = DialogSeverity.Warn,
                    Title = "File already exists",
                    Message = $"'{selectedFile.FileInfo.Name}' already exists in this folder. Do you want to overwrite it?",
                    CancelText = "Cancel",
                    ConfirmText = "Overwrite",
                    OnConfirmAsync = async () => await UploadWithOverwriteOption(selectedFile, true)
                });
                return;
            }

            await _documentService.UploadDocumentWithPresignedUrl(uploadDetails.UploadUrl, selectedFile);
            _messenger.Send(new ShowToast(MessageState.Success, $"Uploaded {selectedFile.FileInfo.Name}"));
            await RefreshAsync();
        }

        private async Task DownloadAsync(DocumentationFileItem item)
        {
            try
            {
                var url = await _documentService.GetDocumentationDownloadUrl(item.Key);
                if (!url.IsWhiteSpace())
                {
                    _navigationService.NavigateTo(url!, forceLoad: true);
                    _messenger.Send(new ShowToast(MessageState.Success, "Download started"));
                }
                else
                {
                    _messenger.Send(new ShowMessage(MessageState.Error, "Download failed", "Unable to get download URL."));
                }
            }
            catch (Exception)
            {
                _messenger.Send(new ShowMessage(MessageState.Error, "Download failed", "Unable to download requested file."));
            }
        }

        public class DocumentationTreeNode
        {
            public string Id { get; set; } = string.Empty;
            public string? ParentId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string IconCss { get; set; } = "fa-solid fa-folder";
            public bool Expanded { get; set; }
            public bool HasChildren { get; set; }
        }

        public class DocumentationFileItem
        {
            public string Key { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public DateTime CreatedUtc { get; set; }
        }
    }
}
