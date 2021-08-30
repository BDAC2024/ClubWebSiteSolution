using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.Linq;

namespace AnglingClubWebServices.DTOs
{
    public class WaterInputDto : WaterBase
    {
        public float Id { get; set; }
        public List<string> MarkerIcons { get; set; }
        public List<string> MarkerLabels { get; set; }

        public List<double> Destination { get; set; }
        public List<double> Markers { get; set; }
        public List<double> Path { get; set; }
    }

    public class WaterUpdateDto : TableBase
    {
        public string Description { get; set; }
        public string Directions { get; set; }
    }

    public class WaterOutputDto : WaterBase
    {
        public float Id { get; set; }
        public string WaterType 
        {
            get 
            {
                return this.Type.EnumDescription();
            } 
        }
        public string AccessType
        {
            get
            {
                return this.Access.EnumDescription();
            }
        }

        public List<Marker> Markers { get; set; } = new List<Marker>();

        public Position Destination { get; set; } 

        public List<Position> Path { get; set; } = new List<Position>();

        public bool HasLimits 
        {
            get 
            {
                return Markers.Any(x => x.Icon.ToLower().Contains("limit"));
            }
        }
    }
}
