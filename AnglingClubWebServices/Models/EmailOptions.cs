using AnglingClubWebServices.Helpers;
using System.IO;

namespace AnglingClubWebServices.Models
{
    public class EmailOptions
    {
        private string _primaryFromAddress;
        private string _fallbackFromAddress;
        private string _primaryEmailBCC;

        public string PrimaryEmailHost { get; set; }
        public int PrimaryEmailPort { get; set; }
        public string PrimaryEmailUsername { get; set; }
        public string PrimaryEmailPassword { get; set; }
        public string PrimaryEmailFromName { get; set; }
        public string PrimaryEmailFromAddress
        {
            get
            {
                return _primaryFromAddress;
            }

            set
            {
                if (RegexUtilities.IsValidEmail(value))
                {
                    _primaryFromAddress = value;
                }
                else
                {
                    throw new System.Exception($"Invalid FromAddress {value}");
                }
            }
        }
        public string PrimaryEmailRepairUrl { get; set; }
        public string PrimaryEmailBCC
        {
            get
            {
                return _primaryEmailBCC;
            }

            set
            {
                if (RegexUtilities.IsValidEmail(value))
                {
                    _primaryEmailBCC = value;
                }
                else
                {
                    throw new System.Exception($"Invalid PrimaryEmailBCC {value}");
                }
            }
        }



        public string FallbackEmailHost { get; set; }
        public int FallbackEmailPort { get; set; }
        public string FallbackEmailUsername { get; set; }
        public string FallbackEmailPassword { get; set; }
        public string FallbackEmailFromName { get; set; }
        public string FallbackEmailFromAddress
        {
            get
            {
                return _fallbackFromAddress;
            }

            set
            {
                if (RegexUtilities.IsValidEmail(value))
                {
                    _fallbackFromAddress = value;
                }
                else
                {
                    throw new System.Exception($"Invalid FromAddress {value}");
                }
            }
        }

    }

    public class ImageAttachment
    {
        public string DataUrl { get; set; }
        public string Filename { get; set; }
    }

    public class StreamAttachment
    {
        public string Filename { get; set; }
        public byte[] Bytes { get; set; }
        //eg text/calendar
        public string ContentType { get; set; }
    }

}
