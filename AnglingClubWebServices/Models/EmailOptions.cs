using AnglingClubWebServices.Helpers;

namespace AnglingClubWebServices.Models
{
    public class EmailOptions
    {
        private string _fromAddress;

        public string PrimaryEmailHost { get; set; }
        public int PrimaryEmailPort { get; set; }
        public string PrimaryEmailUsername { get; set; }
        public string PrimaryEmailPassword { get; set; }
        public string PrimaryEmailFromName { get; set; }
        public string PrimaryEmailFromAddress
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
        public string PrimaryEmailRepairUrl { get; set; }

        public string FallbackEmailHost { get; set; }
        public int FallbackEmailPort { get; set; }
        public string FallbackEmailUsername { get; set; }
        public string FallbackEmailPassword { get; set; }
        public string FallbackEmailFromName { get; set; }
        public string FallbackEmailFromAddress
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
