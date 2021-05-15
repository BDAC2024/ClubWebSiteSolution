using System.ComponentModel.DataAnnotations;

namespace AnglingClubWebServices.Models
{
    public class AuthenticateRequest
    {
        [Required]
        public int MembershipNumber { get; set; }

        [Required]
        public int Pin { get; set; }
    }
}
