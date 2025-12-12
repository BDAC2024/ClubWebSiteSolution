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
        private readonly IEventRepository _eventRepository;
        private readonly ILogger<ReferenceDataRepository> _logger;

        public ReferenceDataRepository(
            IAppSettingRepository appSettingRepository,
            ILoggerFactory loggerFactory,
            IEventRepository eventRepository)
        {
            _appSettingRepository = appSettingRepository;
            _logger = loggerFactory.CreateLogger<ReferenceDataRepository>();
            _eventRepository = eventRepository;
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

            foreach (var season in EnumUtils.SeasonsForMembershipPurchase())
            {
                refData.SeasonsForMembershipPurchase.Add(new SeasonInfo
                {
                    Season = season,
                    Name = season.SeasonName(),
                    Starts = season.SeasonStarts(),
                    Ends = season.SeasonEnds()
                });
            }

            refData.AppSettings = _appSettingRepository.GetAppSettings().Result;

            return refData;

        }

        public ReferenceData GetReferenceDataForDayTickets()
        {
            var refData = GetReferenceData();

            refData.DayTicketMatches = _eventRepository.GetDayTicketMatches().Result;

            return refData;
        }

    }
}
