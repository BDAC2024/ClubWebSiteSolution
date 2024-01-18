using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;

namespace AnglingClubWebServices.Data
{
    public class ReferenceDataRepository : IReferenceDataRepository
    {
        private readonly IAppSettingsRepository _appSettingsRepository;
        private readonly ILogger<ReferenceDataRepository> _logger;

        public ReferenceDataRepository(
            IAppSettingsRepository appSettingsRepository,
            ILoggerFactory loggerFactory)
        {
            _appSettingsRepository = appSettingsRepository;
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

            refData.AppSettings = _appSettingsRepository.GetAppSettings().Result;

            return refData;

        }

    }
}
