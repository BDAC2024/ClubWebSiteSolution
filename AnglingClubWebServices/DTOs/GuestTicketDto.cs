using System;

namespace AnglingClubWebServices.DTOs
{
    public class GuestTicketDto
    {
        public string MembersName { get; set; } = "";
        public string GuestsName { get; set; } = "";
        public DateTime ValidOn { get; set; } = DateTime.MinValue;

        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }
    }
}
