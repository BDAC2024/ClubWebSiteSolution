namespace AnglingClubWebServices.Models
{
    public class RepositoryOptions
    {
        public string AWSAccessId { get; set; }
        public string AWSSecret { get; set; }
        public string SimpleDbDomain { get; set; }
        public string BackupBucket { get; set; }
        public string TmpFilesBucket { get; set; }
        public string DocumentBucket { get; set; }
        public string AWSRegion { get; set; }
        public string SiteUrl { get; set; }
        public string Stage { get; set; }

    }
}
