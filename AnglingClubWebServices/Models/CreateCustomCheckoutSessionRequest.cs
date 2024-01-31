using AnglingClubWebServices.Interfaces;
using System.Collections.Generic;

namespace AnglingClubWebServices.Models
{
    public class CreateCustomCheckoutSessionRequest
    {
        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }
        public string ProductId { get; set; }
        public decimal ProductPrice { get; set; }
        public CheckoutType Mode { get; set; }

        public Dictionary<string, string> MetaData { get; set; }
    }
}
