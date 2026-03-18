using AnglingClubShared.DTOs;
using AnglingClubWebsite.Models;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Navigations;

namespace AnglingClubWebsite.Pages
{
    public partial class Documentation : RazorComponentBase
    {
        private const string RootId = "_root";

        private readonly IDocumentService _documentService;
        private readonly INavigationService _navigationService;
        private readonly IMessenger _messenger;
        private readonly IJSRuntime _jsRuntime;

        public Documentation(
            ICurrentUserService currentUserService,
            IAuthenticationService authenticationService,
            IMessenger messenger,
            IDocumentService documentService,
            INavigationService navigationService,
            IJSRuntime jsRuntime) : base(messenger, currentUserService, authenticationService)
        {
            _documentService = documentService;
            _navigationService = navigationService;
            _messenger = messenger;
            _jsRuntime = jsRuntime;
        }

        public bool DataLoaded { get; set; }
        public List<DocumentationStoredFileDto> AllFiles { get; set; } = [];
        public List<FolderNode> FolderTree { get; set; } = [];
        public string[] SelectedNodes { get; set; } = [RootId];
        public string SelectedFolderPath { get; set; } = "";
        public List<FolderFileView> FilesInSelectedFolder { get; set; } = [];
        public string NewFolderName { get; set; } = "";
        public UploadFiles? SelectedUploadFile { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            DataLoaded = false;
            await LoadAsync();
            DataLoaded = true;
            await base.OnParametersSetAsync();
        }

        private async Task LoadAsync()
        {
            AllFiles = await _documentService.GetDocumentationFiles() ?? [];
            BuildTree();

            if (!FolderTree.Any(x => x.Path == SelectedFolderPath))
            {
                SelectedFolderPath = "";
                SelectedNodes = [RootId];
            }

            RefreshFolderFiles();
        }

        private void BuildTree()
        {
            var folderPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "" };

            foreach (var file in AllFiles)
            {
                var key = file.Key.Trim('/');
                if (string.IsNullOrWhiteSpace(key) || key.StartsWith("Meetings/Minutes", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var segments = key.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var depth = segments.Length - 1;

                for (var i = 0; i < depth; i++)
                {
                    folderPaths.Add(string.Join('/', segments.Take(i + 1)));
                }
            }

            FolderTree = [];
            FolderTree.Add(new FolderNode
            {
                Id = RootId,
                ParentId = null,
                Path = "",
                Text = "Root",
                IconCss = "fa-solid fa-folder-open",
                HasChildren = folderPaths.Any(x => !string.IsNullOrWhiteSpace(x) && !x.Contains('/'))
            });

            var folders = folderPaths.Where(x => !string.IsNullOrWhiteSpace(x)).OrderBy(x => x).ToList();
            foreach (var path in folders)
            {
                var split = path.Split('/');
                var parentPath = split.Length == 1 ? "" : string.Join('/', split.Take(split.Length - 1));
                var parentId = string.IsNullOrWhiteSpace(parentPath) ? RootId : ToNodeId(parentPath);

                FolderTree.Add(new FolderNode
                {
                    Id = ToNodeId(path),
                    ParentId = parentId,
                    Path = path,
                    Text = split.Last(),
                    IconCss = "fa-solid fa-folder",
                    HasChildren = folderPaths.Any(x => x.StartsWith(path + "/", StringComparison.OrdinalIgnoreCase) && x.Count(c => c == '/') == path.Count(c => c == '/') + 1)
                });
            }
        }

        private static string ToNodeId(string path) => $"node_{path.Replace('/', '_')}";

        private void RefreshFolderFiles()
        {
            FilesInSelectedFolder = AllFiles
                .Where(x => !x.Key.EndsWith("/.folder", StringComparison.OrdinalIgnoreCase))
                .Where(x => IsFileInCurrentFolder(x.Key))
                .Select(x => new FolderFileView
                {
                    Key = x.Key,
                    Created = x.Created,
                    FileName = Path.GetFileName(x.Key)
                })
                .ToList();
        }

        private bool IsFileInCurrentFolder(string key)
        {
            var normalized = key.Trim('/');
            if (string.IsNullOrWhiteSpace(SelectedFolderPath))
            {
                return !normalized.Contains('/');
            }

            if (!normalized.StartsWith(SelectedFolderPath + "/", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var remaining = normalized.Substring(SelectedFolderPath.Length + 1);
            return !remaining.Contains('/');
        }

        private void OnFolderClicked(NodeClickEventArgs args)
        {
            var node = FolderTree.FirstOrDefault(x => x.Id == args.NodeData.Id);
            if (node == null)
            {
                return;
            }

            SelectedFolderPath = node.Path;
            SelectedNodes = [node.Id];
            RefreshFolderFiles();
        }

        private async Task CreateFolderAsync()
        {
            if (string.IsNullOrWhiteSpace(NewFolderName))
            {
                _messenger.Send(new ShowMessage(MessageState.Warn, "Folder name required", "Please enter a folder name."));
                return;
            }

            var folderPath = string.IsNullOrWhiteSpace(SelectedFolderPath)
                ? NewFolderName.Trim('/').Trim()
                : $"{SelectedFolderPath}/{NewFolderName.Trim('/').Trim()}";

            await _documentService.CreateDocumentationFolder(folderPath);
            NewFolderName = "";
            await LoadAsync();
            SelectedFolderPath = folderPath;
            SelectedNodes = [ToNodeId(SelectedFolderPath)];
            RefreshFolderFiles();

            _messenger.Send(new ShowToast(MessageState.Success, "Folder created"));
        }

        private Task FileSelectedAsync(UploadChangeEventArgs args)
        {
            SelectedUploadFile = args.Files.FirstOrDefault();
            return Task.CompletedTask;
        }

        private async Task UploadSelectedFileAsync()
        {
            if (SelectedUploadFile == null)
            {
                return;
            }

            try
            {
                var upload = await _documentService.GetDocumentationUploadUrl(SelectedFolderPath, SelectedUploadFile.FileInfo.Name, (SelectedUploadFile.File.ContentType ?? "application/octet-stream"), false);
                await _documentService.UploadDocumentWithPresignedUrl(upload!.UploadUrl, SelectedUploadFile);
            }
            catch (InvalidOperationException ex) when (ex.Message == "FileAlreadyExists")
            {
                var confirmed = await _jsRuntime.InvokeAsync<bool>("confirm", $"'{SelectedUploadFile.FileInfo.Name}' already exists. Overwrite?");
                if (!confirmed)
                {
                    return;
                }

                var upload = await _documentService.GetDocumentationUploadUrl(SelectedFolderPath, SelectedUploadFile.FileInfo.Name, (SelectedUploadFile.File.ContentType ?? "application/octet-stream"), true);
                await _documentService.UploadDocumentWithPresignedUrl(upload!.UploadUrl, SelectedUploadFile);
            }

            _messenger.Send(new ShowToast(MessageState.Success, "Upload complete"));
            SelectedUploadFile = null;
            await LoadAsync();
        }

        private async Task DownloadAsync(string key)
        {
            var url = await _documentService.GetDocumentationDownloadUrl(key);
            if (!string.IsNullOrWhiteSpace(url))
            {
                _navigationService.NavigateTo(url, forceLoad: true);
            }
        }

        public class FolderNode
        {
            public string Id { get; set; } = "";
            public string? ParentId { get; set; }
            public string Path { get; set; } = "";
            public string Text { get; set; } = "";
            public string IconCss { get; set; } = "";
            public bool HasChildren { get; set; }
        }

        public class FolderFileView
        {
            public string Key { get; set; } = "";
            public string FileName { get; set; } = "";
            public DateTime Created { get; set; }
        }
    }
}
