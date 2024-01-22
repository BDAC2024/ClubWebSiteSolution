using System;

namespace AnglingClubWebServices.Interfaces
{
    public interface ITicketService
    {
        void IssueDayTicket(DateTime validOn, string holdersName, string emailAddress, string paymentId);
    }
}