using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IAppSettingsRepository
    {
        Task AddOrUpdateAppSettings(AppSettings appSettings);
        Task<AppSettings> GetAppSettings();
        Task DeleteAppSettings(string id);
    }
}