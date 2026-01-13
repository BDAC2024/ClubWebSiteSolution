namespace AnglingClubShared.Entities
{
    public class NewsItem : TableBase
    {
        public DateTime Date { get; set; }
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";

    }
}
