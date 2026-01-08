namespace AnglingClubShared.DTOs
{
    public class AppSettingListDto
    {
        public List<string> Names { get; set; } = new List<string>();
        public bool AbortOnMissingNames { get; set; } = true;
    }
}
