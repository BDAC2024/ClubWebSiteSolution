using AnglingClubShared.Enums;
using AnglingClubShared.Extensions;
using AnglingClubShared.Models;
using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;

namespace AnglingClubWebServices.Data
{
    public class ReferenceDataRepository : IReferenceDataRepository
    {
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly ILogger<ReferenceDataRepository> _logger;

        public ReferenceDataRepository(
            IAppSettingRepository appSettingRepository,
            ILoggerFactory loggerFactory)
        {
            _appSettingRepository = appSettingRepository;
            _logger = loggerFactory.CreateLogger<ReferenceDataRepository>();
        }

        public ReferenceData GetReferenceData()
        {
            var refData = new ReferenceData();

            foreach (var season in EnumUtils.GetValues<Season>())
            {
                refData.Seasons.Add(new SeasonInfo
                {
                    Season = season,
                    Name = season.SeasonName(),
                    Starts = season.SeasonStarts(),
                    Ends = season.SeasonEnds()
                });
                
            }

            refData.CurrentSeason = EnumUtils.CurrentSeason();

            refData.AppSettings = _appSettingRepository.GetAppSettings().Result;

            return refData;

        }

    }
}
