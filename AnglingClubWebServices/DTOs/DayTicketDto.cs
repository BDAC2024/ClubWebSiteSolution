using System;

namespace AnglingClubWebServices.DTOs
{
    public class DayTicketDto
    {
        public string HoldersName { get; set; } = "";
        public DateTime ValidOn { get; set; } = DateTime.MinValue;

        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }
    }
}
