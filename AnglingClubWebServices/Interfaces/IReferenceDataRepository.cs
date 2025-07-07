using AnglingClubWebServices.Models;

namespace AnglingClubWebServices.Interfaces
{
    public interface IReferenceDataRepository
    {
        ReferenceData GetReferenceData();
        ReferenceData GetReferenceDataForDayTickets();
    }
}