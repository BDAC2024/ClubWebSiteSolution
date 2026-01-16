using AnglingClubShared.Enums;

namespace AnglingClubWebsite.Services
{
    public class GlobalService : IGlobalService
    {
        private Season? _storedSeason = null;

        public Season GetStoredSeason(Season defaultIfEmpty)
        {
            if (_storedSeason == null)
            {
                _storedSeason = defaultIfEmpty;
            }

            return _storedSeason.Value;
        }

        public void SetStoredSeason(Season season)
        {
            _storedSeason = season;
        }

        public bool IsEmbedded { get; set; }
    }
}
