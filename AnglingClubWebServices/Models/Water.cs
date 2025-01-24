using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubWebServices.Interfaces;

namespace AnglingClubWebServices.Models
{
    public class WaterBase : TableBase
    {
        public string Name { get; set; }
        public WaterType Type { get; set; }
        public WaterAccessType Access { get; set; }
        public string Description { get; set; }
        public string Species { get; set; }
        public string Directions { get; set; }
        public string W3wCarPark { get; set; }

        public string VideoShortCode { get; set; }
    }

    public class Water : WaterBase
    {
        public float Id { get; set; }
        
        public string Markers { get; set; }
        public string MarkerIcons { get; set; }
        public string MarkerLabels { get; set; }

        public string Destination { get; set; }
        public string Centre { get; set; }
        public string Path { get; set; }

    }
}
