namespace AnglingClubWebServices.Models
{
    public class AuthOptions
    {
        public string AuthSecretKey { get; set; }
        public int AuthExpireMinutes { get; set; }
        public string DeveloperName { get; set; }

    }
}
