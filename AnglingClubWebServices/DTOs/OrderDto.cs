using AnglingClubWebServices.Models;

namespace AnglingClubShared.DTOs
{
    public class OrderDto
    {
        public string Key { get; set; }
        public string Id { get; set; }

        public string Email { get; set; }
        public int MembershipNumber { get; set; }

        public string CallerBaseUrl { get; set; }
    }
}
