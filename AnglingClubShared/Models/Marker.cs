namespace AnglingClubShared.Models
{
    public class Marker
    {
        public Position Position { get; set; } = new Position();
        public string Label { get; set; } = "";
        public string Icon { get; set; } = "";
    }
}
