using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
using AnglingClubWebsite.Helpers;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components.Forms;
using Syncfusion.Blazor.Navigations;

namespace AnglingClubWebsite.Pages
{
    public partial class Documentation : RazorComponentBase
    {
        private readonly IDocumentationService _documentationService;
        private readonly IDialogQueue _dialogQueue;
        private readonly IMessenger _messenger;
        private readonly INavigationService _navigationService;

        private const string RootNodeId = "root";

        public Documentation(
            ICurrentUserService currentUserService,
            IAuthenticationService authenticationService,
            IDocumentationService documentationService,
            IDialogQueue dialogQueue,
            IMessenger messenger,
            INavigationService navigationService) : base(messenger, currentUserService, authenticationService)
        {
            _documentationService = documentationService;
            _dialogQueue = dialogQueue;
            _messenger = messenger;
            _navigationService = navigationService;
        }

        public bool DataLoaded { get; set; } = false;
        public string NewFolderName { get; set; } = "";
        public string SelectedFolderPath { get; set; } = "";
        public IBrowserFile? SelectedFile { get; set; }
        public string[] ExpandedNodes { get; set; } = new[] { RootNodeId };
        public List<FolderNode> FolderNodes { get; set; } = new List<FolderNode>();
        public List<DocumentationFileItemDto> CurrentFolderFiles { get; set; } = new List<DocumentationFileItemDto>();

        protected override async Task OnParametersSetAsync()
        {
            await LoadAsync();
            await base.OnParametersSetAsync();
        }

        private async Task LoadAsync()
        {
            DataLoaded = false;

            var folderTree = await _documentationService.GetFolderTree();

            FolderNodes = BuildTreeNodes(folderTree?.FolderPaths ?? new List<string>());
            CurrentFolderFiles = await _documentationService.GetFiles(SelectedFolderPath) ?? new List<DocumentationFileItemDto>();

            DataLoaded = true;
        }

        public async Task OnFolderClicked(NodeClickEventArgs args)
        {
            var clickedNodeId = args.NodeData?.Id;

            var selected = FolderNodes.FirstOrDefault(x => x.Id == clickedNodeId);
            SelectedFolderPath = selected?.FolderPath ?? "";

            CurrentFolderFiles = await _documentationService.GetFiles(SelectedFolderPath) ?? new List<DocumentationFileItemDto>();
            StateHasChanged();
        }

        public async Task CreateFolderAsync()
        {
            if (string.IsNullOrWhiteSpace(NewFolderName))
            {
                _messenger.Send(new ShowMessage(MessageState.Warn, "Invalid folder", "Please enter a folder name."));
                return;
            }

            try
            {
                await _documentationService.CreateFolder(new DocumentationCreateFolderRequestDto
                {
                    ParentFolderPath = SelectedFolderPath,
                    FolderName = NewFolderName.Trim()
                });

                NewFolderName = "";
                await LoadAsync();
                _messenger.Send(new ShowToast(MessageState.Success, "Folder created"));
            }
            catch (Exception)
            {
                _messenger.Send(new ShowMessage(MessageState.Error, "Folder create failed", "Unable to create folder."));
            }
        }

        public Task OnFileSelectedAsync(InputFileChangeEventArgs args)
        {
            SelectedFile = args.File;
            return Task.CompletedTask;
        }

        public async Task UploadSelectedFileAsync()
        {
            if (SelectedFile == null)
            {
                return;
            }

            await StartUploadAsync(false);
        }

        private async Task StartUploadAsync(bool overwrite)
        {
            if (SelectedFile == null)
            {
                return;
            }

            try
            {
                var uploadInfo = await _documentationService.GetUploadUrl(new DocumentationUploadUrlRequestDto
                {
                    FolderPath = SelectedFolderPath,
                    FileName = SelectedFile.Name,
                    ContentType = SelectedFile.ContentType,
                    Overwrite = overwrite
                });

                if (uploadInfo == null)
                {
                    _messenger.Send(new ShowMessage(MessageState.Error, "Upload failed", "Unable to prepare upload."));
                    return;
                }

                if (uploadInfo.RequiresOverwriteConfirmation)
                {
                    _dialogQueue.Enqueue(new DialogRequest
                    {
                        Kind = DialogKind.Confirm,
                        Severity = DialogSeverity.Warn,
                        Title = "Overwrite file?",
                        Message = $"A file named '{SelectedFile.Name}' already exists in this folder. Overwrite it?",
                        CancelText = "Cancel",
                        ConfirmText = "Overwrite",
                        OnConfirmAsync = async () =>
                        {
                            await StartUploadAsync(true);
                        }
                    });

                    return;
                }

                await _documentationService.UploadWithPresignedUrl(uploadInfo.UploadUrl, SelectedFile);
                SelectedFile = null;
                await LoadAsync();

                _messenger.Send(new ShowToast(MessageState.Success, "File uploaded"));
            }
            catch (S3UploadException ex)
            {
                _messenger.Send(new ShowMessage(MessageState.Error, "Upload failed", ex.UserMessage));
            }
            catch (Exception)
            {
                _messenger.Send(new ShowMessage(MessageState.Error, "Upload failed", "Unable to upload selected file."));
            }
        }

        public async Task DownloadFileAsync(DocumentationFileItemDto file)
        {
            try
            {
                var url = await _documentationService.GetDownloadUrl(file.Key);

                if (!string.IsNullOrWhiteSpace(url))
                {
                    _navigationService.NavigateTo(url, true);
                }
                else
                {
                    _messenger.Send(new ShowMessage(MessageState.Error, "Download failed", "Unable to generate download url."));
                }
            }
            catch (Exception)
            {
                _messenger.Send(new ShowMessage(MessageState.Error, "Download failed", "Unable to download selected file."));
            }
        }

        public string DisplayFolder(string folderPath)
        {
            return string.IsNullOrWhiteSpace(folderPath) ? "(root)" : folderPath;
        }

        private static List<FolderNode> BuildTreeNodes(List<string> folderPaths)
        {
            var nodes = new List<FolderNode>
            {
                new FolderNode
                {
                    Id = RootNodeId,
                    ParentId = null,
                    Name = "Documentation",
                    FolderPath = "",
                    HasChildren = true,
                    IconCss = "fa-solid fa-folder"
                }
            };

            var ordered = folderPaths
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            foreach (var path in ordered)
            {
                var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var currentPath = "";

                for (var i = 0; i < parts.Length; i++)
                {
                    currentPath = string.IsNullOrWhiteSpace(currentPath) ? parts[i] : $"{currentPath}/{parts[i]}";
                    var id = $"folder:{currentPath}";

                    if (nodes.Any(x => x.Id == id))
                    {
                        continue;
                    }

                    var parentPath = i == 0 ? "" : string.Join('/', parts.Take(i));
                    var parentId = string.IsNullOrWhiteSpace(parentPath) ? RootNodeId : $"folder:{parentPath}";

                    nodes.Add(new FolderNode
                    {
                        Id = id,
                        ParentId = parentId,
                        Name = parts[i],
                        FolderPath = currentPath,
                        HasChildren = true,
                        IconCss = "fa-solid fa-folder"
                    });
                }
            }

            return nodes;
        }

        public class FolderNode
        {
            public string Id { get; set; } = "";
            public string? ParentId { get; set; }
            public string Name { get; set; } = "";
            public string FolderPath { get; set; } = "";
            public bool HasChildren { get; set; }
            public string IconCss { get; set; } = "";
        }
    }
}
