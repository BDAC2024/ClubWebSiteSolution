using AnglingClubWebServices.Interfaces;

namespace AnglingClubWebServices.Models
{
    public class Rules : TableBase
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public RuleType RuleType { get; set; }

    }
}
