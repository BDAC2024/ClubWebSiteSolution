using AnglingClubShared.Enums;
using AnglingClubWebServices.Interfaces;
using System.Collections.Generic;

namespace AnglingClubWebServices.Models
{
    public class CreateCheckoutSessionRequest
    {
        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }
        public string PriceId { get; set; }
        public CheckoutType Mode { get; set; }

        public Dictionary<string, string> MetaData { get; set; }
    }
}
