using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IAppSettingRepository
    {
        Task AddOrUpdateAppSettings(AppSettings appSettings);
        Task AddOrUpdateAppSetting(AppSetting appSetting);
        Task<AppSettings> GetAppSettings();
        Task DeleteAppSetting(string id);
    }
}