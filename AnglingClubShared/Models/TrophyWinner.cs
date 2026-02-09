using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubShared.Extensions;
using MatchType = AnglingClubShared.Enums.MatchType;

namespace AnglingClubShared.Models
{

    public class TrophyWinnerBase : TableBase
    {
        public string Trophy { get; set; } = "";
        public TrophyType TrophyType { get; set; }
        public AggregateType? AggregateType { get; set; }
        public MatchType? MatchType { get; set; }

        /// <summary>
        /// Is this a long-running trophy e.g. non known until end of season
        /// </summary>
        public bool IsRunning { get; set; }

        public string Winner { get; set; } = "";
        public float WeightDecimal { get; set; }
        public float Points { get; set; }
        public string Venue { get; set; } = "";
        public DateTime? Date { get; set; }
        public string DateDesc { get; set; } = "";
        public Season Season { get; set; }

    }

    public class TrophyWinner : TrophyWinnerBase
    {
        public string WeightPoints {
            get
            {


                var wtPts = "";

                if (WeightDecimal > 0)
                {
                    wtPts = WeightDecimal.WeightAsString();
                }
                else if (Points > 0)
                {
                    wtPts = $"{this.Points} points";
                }
                else
                {
                    wtPts = "";
                }

                return wtPts;

            }

        }

        public string DateSummary {
            get
            {
                var dt = "";

                if (this.Date != null)
                {
                    dt = Date.Value.ToString("dd MMM yy");
                }
                else
                {
                    dt = DateDesc;
                }

                return dt;
            }
        }
    }


}
