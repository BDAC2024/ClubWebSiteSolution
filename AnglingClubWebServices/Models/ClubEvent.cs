using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using System;

namespace AnglingClubWebServices.Models
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
        public string Description { get; set; }
        public string Cup { get; set; }
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
                if (EventType != EventType.Match)
                {
                    var formatted = Date.ToString("HH:mm");
                    return formatted == "00:00" ? "" : formatted;
                }
                else
                {
                    return "";
                }
            }
        }

        public string DescriptionForTable 
        { 
            get
            {
                if (EventType == EventType.Work)
                {
                    return $"{EventType.EnumDescription()} at {Description}";
                }
                else if (EventType == EventType.Match)
                {
                    return $"{MatchType.EnumDescription()} {(Number != null ? $"no.{Number}" : "")} at {Description}";
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
                return MatchEnd != null ? MatchEnd < DateTime.Now :  Date < DateTime.Now.Date;
            }
        }
    }

}
