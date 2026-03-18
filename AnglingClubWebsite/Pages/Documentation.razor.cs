using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
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
        private readonly IMessenger _messenger;
        private readonly IDialogQueue _dialogQueue;
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
            _messenger = messenger;
            _dialogQueue = dialogQueue;
            _navigationService = navigationService;
        }

        public bool DataLoaded { get; set; }
        public string CurrentFolderPath { get; set; } = string.Empty;
        public string NewFolderName { get; set; } = string.Empty;
        public string[] SelectedNodeIds { get; set; } = ["root"];
        public List<DocumentationNode> TreeNodes { get; set; } = new();
        public List<DocumentationFileItemDto> Files { get; set; } = new();
        public UploadFiles? SelectedUploadFile { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await LoadFolderAsync(CurrentFolderPath);
            DataLoaded = true;
            await base.OnParametersSetAsync();
        }

        private async Task LoadFolderAsync(string folderPath)
        {
            DataLoaded = false;
            CurrentFolderPath = folderPath?.Trim('/') ?? string.Empty;

            var listing = await _documentService.GetDocumentationListing(CurrentFolderPath) ?? new DocumentationListingDto();
            Files = listing.Files;
            BuildTreeNodes(listing.Folders);

            DataLoaded = true;
            StateHasChanged();
        }

        private void BuildTreeNodes(List<string> allFolders)
        {
            var nodes = new List<DocumentationNode>
            {
                new DocumentationNode
                {
                    Id = "root",
                    ParentId = null,
                    Text = "Documentation",
                    FolderPath = string.Empty,
                    Expanded = true,
                    HasChildren = true,
                    IconCss = "fa-solid fa-folder-tree"
                }
            };

            var folderSet = new HashSet<string>(allFolders, StringComparer.OrdinalIgnoreCase);

            foreach (var folderPath in folderSet.OrderBy(x => x))
            {
                var parts = folderPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var current = string.Empty;

                for (var i = 0; i < parts.Length; i++)
                {
                    current = string.IsNullOrWhiteSpace(current) ? parts[i] : $"{current}/{parts[i]}";
                    if (nodes.Any(x => x.Id == current))
                    {
                        continue;
                    }

                    var parentPath = i == 0 ? "root" : current[..current.LastIndexOf('/')];
                    var hasChildren = folderSet.Any(x => x.StartsWith($"{current}/", StringComparison.OrdinalIgnoreCase));

                    nodes.Add(new DocumentationNode
                    {
                        Id = current,
                        ParentId = parentPath,
                        Text = parts[i],
                        FolderPath = current,
                        Expanded = IsParentOfCurrent(current),
                        HasChildren = hasChildren,
                        IconCss = hasChildren ? "fa-solid fa-folder-tree" : "fa-solid fa-folder"
                    });
                }
            }

            TreeNodes = nodes;
            SelectedNodeIds = [string.IsNullOrWhiteSpace(CurrentFolderPath) ? "root" : CurrentFolderPath];
        }

        private bool IsParentOfCurrent(string path)
        {
            if (string.IsNullOrWhiteSpace(CurrentFolderPath))
            {
                return false;
            }

            return CurrentFolderPath.StartsWith($"{path}/", StringComparison.OrdinalIgnoreCase) ||
                   CurrentFolderPath.Equals(path, StringComparison.OrdinalIgnoreCase);
        }

        private async Task NodeClicked(NodeClickEventArgs args)
        {
            var selectedId = args.NodeData?.Id ?? "root";
            if (selectedId == "root")
            {
                await LoadFolderAsync(string.Empty);
                return;
            }

            await LoadFolderAsync(selectedId);
        }

        private async Task CreateFolderAsync()
        {
            var folderName = (NewFolderName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(folderName))
            {
                _messenger.Send(new ShowMessage(MessageState.Warn, "Folder name required", "Please enter a folder name."));
                return;
            }

            await _documentService.CreateDocumentationFolder(CurrentFolderPath, folderName);
            NewFolderName = string.Empty;
            await LoadFolderAsync(CurrentFolderPath);
            _messenger.Send(new ShowToast(MessageState.Success, "Folder created"));
        }

        private async Task UploadChanged(UploadChangeEventArgs args)
        {
            SelectedUploadFile = args.Files.FirstOrDefault();
            await Task.CompletedTask;
        }

        private async Task RemoveUpload()
        {
            SelectedUploadFile = null;
            await Task.CompletedTask;
        }

        private async Task UploadAsync()
        {
            if (SelectedUploadFile == null)
            {
                return;
            }

            await UploadInternalAsync(overwriteIfExists: false);
        }

        private async Task UploadInternalAsync(bool overwriteIfExists)
        {
            var uploadResult = await _documentService.GetDocumentationUploadUrl(CurrentFolderPath, SelectedUploadFile!, overwriteIfExists);
            if (uploadResult == null)
            {
                _messenger.Send(new ShowMessage(MessageState.Error, "Upload failed", "Unable to get upload url."));
                return;
            }

            if (uploadResult.FileExists && !overwriteIfExists)
            {
                _dialogQueue.Enqueue(new DialogRequest
                {
                    Kind = DialogKind.Confirm,
                    Severity = DialogSeverity.Warn,
                    Title = "Overwrite file?",
                    Message = $"A file named '{SelectedUploadFile!.FileInfo.Name}' already exists in this folder. Do you want to overwrite it?",
                    CancelText = "Cancel",
                    ConfirmText = "Overwrite",
                    OnConfirmAsync = async () => await UploadInternalAsync(overwriteIfExists: true)
                });
                return;
            }

            if (string.IsNullOrWhiteSpace(uploadResult.UploadUrl))
            {
                _messenger.Send(new ShowMessage(MessageState.Error, "Upload failed", "No upload URL was returned."));
                return;
            }

            await _documentService.UploadDocumentWithPresignedUrl(uploadResult.UploadUrl, SelectedUploadFile!);
            SelectedUploadFile = null;
            await LoadFolderAsync(CurrentFolderPath);
            _messenger.Send(new ShowToast(MessageState.Success, "File uploaded"));
        }

        private async Task DownloadAsync(DocumentationFileItemDto file)
        {
            var url = await _documentService.GetDocumentationDownloadUrl(file.Key);
            if (url == null)
            {
                _messenger.Send(new ShowMessage(MessageState.Error, "Download failed", "Unable to get download url."));
                return;
            }

            _navigationService.NavigateTo(url, forceLoad: true);
        }

        public class DocumentationNode
        {
            public string Id { get; set; } = string.Empty;
            public string? ParentId { get; set; }
            public string Text { get; set; } = string.Empty;
            public string FolderPath { get; set; } = string.Empty;
            public bool Expanded { get; set; }
            public bool HasChildren { get; set; }
            public string IconCss { get; set; } = string.Empty;
        }
    }
}
