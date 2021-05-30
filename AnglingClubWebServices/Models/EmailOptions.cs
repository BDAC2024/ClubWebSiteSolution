using AnglingClubWebServices.Helpers;

namespace AnglingClubWebServices.Models
{
    public class EmailOptions
    {
        private string _fromAddress;

        public string EmailHost { get; set; }
        public int EmailPort { get; set; }
        public string EmailUsername { get; set; }
        public string EmailPassword { get; set; }
        public string EmailFromName { get; set; }
        public string EmailFromAddress
        {
            get
            {
                return _fromAddress;
            }

            set
            {
                if (RegexUtilities.IsValidEmail(value))
                {
                    _fromAddress = value;
                }
                else
                {
                    throw new System.Exception($"Invalid FromAddress {value}");
                }
            }
        }
    }
}
