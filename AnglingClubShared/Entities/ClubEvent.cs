using AnglingClubShared.Enums;
using AnglingClubShared.Extensions;
using MatchType = AnglingClubShared.Enums.MatchType;

namespace AnglingClubShared.Entities
{

    public class ClubEventBase : TableBase
    {
        public string Id { get; set; }
        public Season Season { get; set; }
        public DateTime Date { get; set; }
        public EventType EventType { get; set; }
        public MatchType? MatchType { get; set; }
        public AggregateType? AggregateType { get; set; }
        public DateTime? MatchDraw { get; set; }
        public DateTime? MatchStart { get; set; }
        public DateTime? MatchEnd { get; set; }
        public int? Number { get; set; }
        public string Description { get; set; } = "";
        public string Cup { get; set; } = "";
    }



    public class ClubEvent : ClubEventBase
    {
        public string Day
        {
            get
            {
                return Date.ToString("ddd");
            }
        }

        public string Time
        {
            get
            {
                //if (EventType != EventType.Match)
                //{
                    var formatted = Date.ToString("HH:mm");
                    return formatted == "00:00" ? "" : formatted;
                //}
                //else
                //{
                //    return "";
                //}
            }
        }

        public string DescriptionForTable
        {
            get
            {
                if (EventType == EventType.Work)
                {
                    return $"{Description}";
                }
                else if (EventType == EventType.Match)
                {
                    switch (MatchType)
                    {
                        case AnglingClubShared.Enums.MatchType.Club:
                        case AnglingClubShared.Enums.MatchType.Junior:
                        case AnglingClubShared.Enums.MatchType.Pairs:
                        case AnglingClubShared.Enums.MatchType.Evening:
                        case AnglingClubShared.Enums.MatchType.Spring:
                            return $"{MatchType.EnumDescription()} {(Number != null ? $"no.{Number}" : "")} at {Description}";

                        case AnglingClubShared.Enums.MatchType.Specials:
                            return Description;

                        default:
                            return $"{MatchType!.EnumDescription()} - {Description}";
                    }
                }
                else
                {
                    return Description;
                }
            }
        }

        public bool InThePast
        {
            get
            {
                return MatchEnd != null ? MatchEnd < DateTime.Now : Date < DateTime.Now.Date;
            }
        }
    }

}
