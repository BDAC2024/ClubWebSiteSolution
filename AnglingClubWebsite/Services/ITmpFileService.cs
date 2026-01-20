using AnglingClubShared.DTOs;
using Syncfusion.Blazor.Inputs;

namespace AnglingClubWebsite.Services
{
    public interface ITmpFileService
    {
        Task<FileUploadUrlResult?> GetFileUploadUrl(UploadFiles file, string path);
        Task UploadFileWithPresignedUrl(string uploadUrl, UploadFiles selectedFile);
    }
}