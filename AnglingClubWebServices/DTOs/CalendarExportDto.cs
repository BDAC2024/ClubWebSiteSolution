using AnglingClubShared.Enums;
using AnglingClubWebServices.Interfaces;

namespace AnglingClubWebServices.DTOs
{
    public class CalendarExportDto
    {
        public Season Season { get; set; }
        public string Email { get; set; }
        public CalendarExportType[] selectedCalendarExportTypes { get; set; }
    }
}
