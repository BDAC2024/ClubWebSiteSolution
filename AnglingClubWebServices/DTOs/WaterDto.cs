using AnglingClubWebServices.Models;
using System.Collections.Generic;

namespace AnglingClubWebServices.DTOs
{
    public class WaterInputDto : WaterBase
    {
        public int Id { get; set; }
        public List<string> Icon { get; set; }
        public List<string> Label { get; set; }

        public List<double> Destination { get; set; }
        public List<double> Path { get; set; }
    }

    public class WaterOutputDto : WaterBase
    {
        public int Id { get; set; }
        public List<string> Icon { get; set; }
        public List<string> Label { get; set; }

        public List<Position> Destination { get; set; }
        public List<Position> Path { get; set; }

    }
}
