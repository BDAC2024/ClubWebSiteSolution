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

    }

    public class Water : WaterBase
    {
        public int Id { get; set; }
        public string Icon { get; set; }
        public string Label { get; set; }

        public string Destination { get; set; }
        public string Path { get; set; }

    }
}
