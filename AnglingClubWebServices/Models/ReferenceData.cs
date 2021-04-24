using AnglingClubWebServices.Interfaces;
using System;
using System.Collections.Generic;

namespace AnglingClubWebServices.Models
{
    public class ReferenceData
    {
        public List<SeasonInfo> Seasons { get; set; } = new List<SeasonInfo>();
    }

    public class SeasonInfo
    {
        public Season Season { get; set; }
        public string Name { get; set; }
        public DateTime Starts { get; set; }
        public DateTime Ends { get; set; }
    }

}
